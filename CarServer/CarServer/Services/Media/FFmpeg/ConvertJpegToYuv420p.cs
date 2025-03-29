using FFmpeg.AutoGen;
using System.Runtime.InteropServices;

namespace FfmpegDemo.Services.FFmpeg
{
    public unsafe class ConvertBufferToYuv420p
    {
        private AVCodecContext* _codecContextPtr;        
        private AVPacket* _packetPtr;

        private int _dstWidth, _dstHeight;        

        public ConvertBufferToYuv420p(int dstWidth = default, int dstHeight = default)
        {
            _dstWidth = dstWidth;
            _dstHeight = dstHeight;

            AVCodec* jpegCodec = ffmpeg.avcodec_find_decoder(AVCodecID.AV_CODEC_ID_MJPEG);
            if (jpegCodec == null)
                throw new Exception("Cannot find codec MJPEG!");

            _codecContextPtr = ffmpeg.avcodec_alloc_context3(jpegCodec);
            if (ffmpeg.avcodec_open2(_codecContextPtr, jpegCodec, null) < 0)
                throw new Exception("Cannot open codec MJPEG!");

            _packetPtr = ffmpeg.av_packet_alloc();
        }

        private AVFrame* UnzipBufferToFrame(byte[] buffer)
        {
            // Convert byte[] to AVPacket
            AVFrame* frame = ffmpeg.av_frame_alloc();

            ffmpeg.av_new_packet(_packetPtr, buffer.Length);
            Marshal.Copy(buffer, 0, (IntPtr)_packetPtr->data, buffer.Length);

            // Decode to AVFrame
            if (ffmpeg.avcodec_send_packet(_codecContextPtr, _packetPtr) < 0)
                return null;

            if (ffmpeg.avcodec_receive_frame(_codecContextPtr, frame) < 0)
                return null;

            // Delete data in AVPacket
            ffmpeg.av_packet_unref(_packetPtr);
            return frame;
        }

        public AVFrame* ConvertToYuv420PFrame(byte[] jpegBuffer)
        {
            AVFrame* jpegFrame = UnzipBufferToFrame(jpegBuffer);
            if (jpegFrame == null)
                return null;

            AVFrame* yuv420pFrame = ffmpeg.av_frame_alloc();

            yuv420pFrame->format = (int)AVPixelFormat.AV_PIX_FMT_YUV420P;
            yuv420pFrame->width = _dstWidth;
            yuv420pFrame->height = _dstHeight;
            ffmpeg.av_frame_get_buffer(yuv420pFrame, 0);

            SwsContext* swsCtxYUV = ffmpeg.sws_getContext(
                jpegFrame->width, jpegFrame->height, AVPixelFormat.AV_PIX_FMT_YUVJ420P,
                _dstWidth, _dstHeight, AVPixelFormat.AV_PIX_FMT_YUV420P,
                ffmpeg.SWS_BILINEAR, null, null, null
            );


            ffmpeg.sws_scale(swsCtxYUV,
                            jpegFrame->data, jpegFrame->linesize,
                            0, _dstHeight,
                            yuv420pFrame->data, yuv420pFrame->linesize);

            // Free up memory
            ffmpeg.av_frame_free(&jpegFrame);
            ffmpeg.sws_freeContext(swsCtxYUV);
            return yuv420pFrame;
        }

        public void Cleanup()
        {
            fixed (AVPacket** packetTemp = &_packetPtr)
                ffmpeg.av_packet_free(packetTemp);

            fixed (AVCodecContext** codecContextTemp = &_codecContextPtr)
                ffmpeg.avcodec_free_context(codecContextTemp);
        }
    }
}
