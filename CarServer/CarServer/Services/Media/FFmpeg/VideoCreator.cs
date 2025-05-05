using FFmpeg.AutoGen;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace CarServer.Services.Media.FFmpeg
{
    public unsafe class VideoCreator
    {
        private readonly ConcurrentQueue<byte[]> _bufferQueue = new();

        private readonly ContainerFormat _containerFormat;
        private readonly ConvertBufferToYuv420p _convertBufferToYuv420p;
        private readonly Stopwatch _stopwatch;


        private readonly AVFormatContext* _formatContextPtr;
        private readonly AVCodecContext* _codecContextPtr;
        private readonly AVStream* _streamPtr;
        private readonly AVPacket* _packet;
        VfrPtsGenerator _vfrPts = new VfrPtsGenerator(); // time_base.den = 90000

        private bool _isRunning;

        public VideoCreator(string outputFile, int dstWidth, int dstHeight)
        {
            _containerFormat = new ContainerFormat(outputFile, dstWidth, dstHeight);
            _convertBufferToYuv420p = new ConvertBufferToYuv420p(dstWidth, dstHeight);

            _formatContextPtr = _containerFormat.GetFormatContext();
            _codecContextPtr = _containerFormat.GetCodecContext();
            _streamPtr = _containerFormat.GetStream();
            _packet = ffmpeg.av_packet_alloc();
            _isRunning = true;
            _stopwatch = Stopwatch.StartNew();

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
                        Thread.Sleep(200); // Tránh CPU chạy 100%
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
            AVFrame* yuv420pFrame = _convertBufferToYuv420p.ConvertToYuv420PFrame(buffer);
            if (yuv420pFrame == null)
                return;

            yuv420pFrame->pts = _vfrPts.GetPts();
            yuv420pFrame->duration = 1;
            
            Console.WriteLine($"Frame pts = {yuv420pFrame->pts}, duration = {yuv420pFrame->duration}");


            if (ffmpeg.avcodec_send_frame(_codecContextPtr, yuv420pFrame) < 0)
                throw new Exception("Error when sending frame!");


            while (ffmpeg.avcodec_receive_packet(_codecContextPtr, _packet) == 0)
            {
                ffmpeg.av_packet_rescale_ts(_packet, _codecContextPtr->time_base, _streamPtr->time_base);
                _packet->stream_index = _streamPtr->index;

//                Console.WriteLine($"Frame PTS: {yuv420pFrame->pts}; Packet PTS: {_packet->pts}, DTS: {_packet->dts}");

                ffmpeg.av_interleaved_write_frame(_formatContextPtr, _packet);
                ffmpeg.av_packet_unref(_packet);
            }

            // Giải phóng frame
            ffmpeg.av_frame_free(&yuv420pFrame);
        }

        private void FinalizeEncoding()
        {
            // Send null value to encoder flush buffer
            ffmpeg.avcodec_send_frame(_codecContextPtr, null);
            while (ffmpeg.avcodec_receive_packet(_codecContextPtr, _packet) == 0)
            {
                ffmpeg.av_packet_rescale_ts(_packet, _codecContextPtr->time_base, _streamPtr->time_base);
                _packet->stream_index = _streamPtr->index;
                ffmpeg.av_interleaved_write_frame(_formatContextPtr, _packet);
                ffmpeg.av_packet_unref(_packet);
            }

            // Write footer
            ffmpeg.av_write_trailer(_containerFormat.GetFormatContext());

            // Free up memory
            _containerFormat.Cleanup();
            _convertBufferToYuv420p.Cleanup();
            fixed (AVPacket** packetTemp = &_packet)
                ffmpeg.av_packet_free(packetTemp);
        }
    }
}
