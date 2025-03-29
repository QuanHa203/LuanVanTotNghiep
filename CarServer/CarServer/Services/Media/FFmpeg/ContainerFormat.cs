using FFmpeg.AutoGen;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.IO;

namespace CarServer.Services.Media.FFmpeg
{
    public unsafe class ContainerFormat
    {
        private AVFormatContext* _formatContextPtr;
        private AVCodecContext* _codecContextPtr;
        private AVStream* _streamPtr;
        private static bool isLocateFFmpegLibrary = false;
        private string _outputFile;
        private int _width, _height, _fps;


        public ContainerFormat(string outputFile, int width, int height, int fps)
        {
            if (!isLocateFFmpegLibrary)
            {
                FFmpegInitialize();
                isLocateFFmpegLibrary = true;
            }

            _width = width;
            _height = height;
            _fps = fps;
            _outputFile = outputFile;

            CreateAVFormatContext();
            CreateAVCodecContext();
            CreateAVStream();
            OpenOutputFile();
        }

        public AVFormatContext* GetFormatContext() => _formatContextPtr;

        public AVCodecContext* GetCodecContext() => _codecContextPtr;

        public AVStream* GetStream() => _streamPtr;

        public int GetWidth() => _width;
        public int GetHeight() => _height;
        public int GetFps() => _fps;

        private void FFmpegInitialize()
        {
            string ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FFmpeg");
            if (!Directory.Exists(ffmpegPath))
                throw new DirectoryNotFoundException($"Cannot find folder FFmpeg: {ffmpegPath}");

            // Locate lib FFmpeg
            ffmpeg.RootPath = ffmpegPath;

            // Initialize FFmpeg to support stream to internet
            ffmpeg.avformat_network_init();
        }

        private void CreateAVFormatContext()
        {
            // Allocate memory for format context
            fixed (AVFormatContext** tempFormatContext = &_formatContextPtr)
                ffmpeg.avformat_alloc_output_context2(tempFormatContext, null, "mp4", _outputFile);

            if (_formatContextPtr == null)
                throw new Exception("Cannot create format context");
        }

        private void CreateAVCodecContext()
        {
            AVCodec* codecPtr = ffmpeg.avcodec_find_encoder(AVCodecID.AV_CODEC_ID_H264);
            if (codecPtr == null)
                throw new Exception("Cannot find H.264");

            _codecContextPtr = ffmpeg.avcodec_alloc_context3(codecPtr);
            if (_codecContextPtr == null)
                throw new Exception("Cannot allocate memory for codec context");

            // Configuration codec
            _codecContextPtr = ffmpeg.avcodec_alloc_context3(codecPtr);
            _codecContextPtr->width = _width;
            _codecContextPtr->height = _height;
            _codecContextPtr->time_base = new AVRational { num = 1, den = _fps };
            _codecContextPtr->framerate = new AVRational { num = _fps, den = 1 };
            _codecContextPtr->pix_fmt = AVPixelFormat.AV_PIX_FMT_YUV420P;

            // Configuration 'preset' to encrypt faster
            ffmpeg.av_opt_set(_codecContextPtr->priv_data, "preset", "ultrafast", 0);

            // Configuration bitrate
            AVDictionary* codecOptions = null;
            ffmpeg.av_dict_set(&codecOptions, "tune", "zerolatency", 0);

            // Open codec
            if (ffmpeg.avcodec_open2(_codecContextPtr, codecPtr, &codecOptions) < 0)
                throw new Exception("Cannot open codec H.264");

        }

        private void CreateAVStream()
        {
            // Add AVStream to file
            _streamPtr = ffmpeg.avformat_new_stream(_formatContextPtr, null);
            if (_streamPtr == null)
                throw new Exception("Cannot create AVStream");

            ffmpeg.avcodec_parameters_from_context(_streamPtr->codecpar, _codecContextPtr);
            _streamPtr->time_base = _codecContextPtr->time_base;
        }

        private void OpenOutputFile()
        {
            // Check if format support file output
            if ((_formatContextPtr->oformat->flags & ffmpeg.AVFMT_NOFILE) == 0)
            {
                if (ffmpeg.avio_open(&_formatContextPtr->pb, _outputFile, ffmpeg.AVIO_FLAG_WRITE) < 0)
                    throw new Exception("Cannot open file output, please check file format");
            }

            // Write header in file
            if (ffmpeg.avformat_write_header(_formatContextPtr, null) < 0)
                throw new Exception("Error writing header file");

        }

        public void Cleanup()
        {

            fixed (AVCodecContext** codecContextTemp = &_codecContextPtr)
                ffmpeg.avcodec_free_context(codecContextTemp);


            ffmpeg.avio_closep(&_formatContextPtr->pb);
            fixed (AVFormatContext** formatContextTemp = &_formatContextPtr)
                ffmpeg.avformat_close_input(formatContextTemp);

            ffmpeg.avformat_free_context(_formatContextPtr);
        }
    }
}
