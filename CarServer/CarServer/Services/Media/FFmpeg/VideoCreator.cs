using FFmpeg.AutoGen;
using FfmpegDemo.Services.FFmpeg;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NuGet.Common;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;

namespace CarServer.Services.Media.FFmpeg
{
    public unsafe class VideoCreator
    {
        private readonly ConcurrentQueue<byte[]> _bufferQueue = new();

        private readonly ContainerFormat _containerFormat;
        private readonly ConvertBufferToYuv420p _convertJpegToYuv420P;

        private readonly AVFormatContext* _formatContextPtr;
        private readonly AVCodecContext* _codecContextPtr;
        private readonly AVStream* _streamPtr;
        private readonly AVPacket* _packet;

        private int _fps;
        private int _frameIndex = 0;
        private bool _isRunning;

        public VideoCreator(string outputFile, int dstWidth, int dstHeight, int fps)
        {
            _containerFormat = new ContainerFormat(outputFile, dstWidth, dstHeight, fps);
            _convertJpegToYuv420P = new ConvertBufferToYuv420p(dstWidth, dstHeight);

            _formatContextPtr = _containerFormat.GetFormatContext();
            _codecContextPtr = _containerFormat.GetCodecContext();
            _streamPtr = _containerFormat.GetStream();
            _packet = ffmpeg.av_packet_alloc();
            _isRunning = true;
            _fps = fps;

            _ = Task.Run(() =>
            {
                while (_isRunning || !_bufferQueue.IsEmpty)
                {
                    if (_bufferQueue.TryDequeue(out var buffer))
                    {
                        WriteFrame(buffer);
                    }
                    else
                    {
                        Thread.Sleep(100); // Tránh CPU chạy 100%
                    }
                }

                FinalizeEncoding();
            });
        }

        public void AddFrame(byte[] buffer)
            => _bufferQueue.Enqueue(buffer);

        public void Finish()
            => _isRunning = false;

        private void WriteFrame(byte[] buffer)
        {
            AVFrame* yuv420pFrame = _convertJpegToYuv420P.ConvertToYuv420PFrame(buffer);
            if (yuv420pFrame == null)
                return;
            
            yuv420pFrame->pts = _frameIndex * ffmpeg.av_rescale_q(1, new AVRational { num = 1, den = 30 }, _codecContextPtr->time_base);
            _frameIndex++;
            long pts = _frameIndex * _codecContextPtr->time_base.den / (_fps * _codecContextPtr->time_base.num);

            // Send frame in encoder
            if (ffmpeg.avcodec_send_frame(_containerFormat.GetCodecContext(), yuv420pFrame) < 0)
                throw new Exception("Error when send frame in encoder!");

            // Get packet from encoder
            while (ffmpeg.avcodec_receive_packet(_codecContextPtr, _packet) == 0)
            {
                _packet->stream_index = _streamPtr->index;
                _packet->pts = ffmpeg.av_rescale_q(pts, _codecContextPtr->time_base, _streamPtr->time_base);
                _packet->dts = _packet->pts;

                ffmpeg.av_interleaved_write_frame(_formatContextPtr, _packet);
                ffmpeg.av_packet_unref(_packet);
            }

            // Free up memory
            ffmpeg.av_frame_free(&yuv420pFrame);
        }

        private void FinalizeEncoding()
        {
            // Send null value to encoder flush buffer
            ffmpeg.avcodec_send_frame(_codecContextPtr, null);
            while (ffmpeg.avcodec_receive_packet(_codecContextPtr, _packet) == 0)
            {
                _packet->stream_index = _streamPtr->index;
                _packet->pts = ffmpeg.av_rescale_q(_packet->pts, _codecContextPtr->time_base, _streamPtr->time_base);
                _packet->dts = ffmpeg.av_rescale_q(_packet->dts, _codecContextPtr->time_base, _streamPtr->time_base);
                ffmpeg.av_interleaved_write_frame(_formatContextPtr, _packet);
                ffmpeg.av_packet_unref(_packet);
            }

            // Write footer
            ffmpeg.av_write_trailer(_containerFormat.GetFormatContext());

            // Free up memory
            _containerFormat.Cleanup();
            _convertJpegToYuv420P.Cleanup();
            fixed (AVPacket** packetTemp = &_packet)
                ffmpeg.av_packet_free(packetTemp);
        }
    }
}
