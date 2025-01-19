using FFmpeg.AutoGen;

namespace AutoTf.CentralBridgeOS.Services;

public class CameraService
{
	public CameraService()
	{
		
	}

	public unsafe void StartCapture()
    {
        string outputFileName = "output.mp4";
        string deviceName = "/dev/video0";

        AVFormatContext* inputFormatContext = null;
        if (ffmpeg.avformat_open_input(&inputFormatContext, deviceName, null, null) != 0)
            throw new ApplicationException("Could not open video source.");

        if (ffmpeg.avformat_find_stream_info(inputFormatContext, null) < 0)
            throw new ApplicationException("Could not find stream information.");

        AVStream* inputVideoStream = null;
        for (int i = 0; i < inputFormatContext->nb_streams; i++)
        {
            if (inputFormatContext->streams[i]->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_VIDEO)
            {
                inputVideoStream = inputFormatContext->streams[i];
                break;
            }
        }

        if (inputVideoStream == null)
            throw new ApplicationException("Failed to find a video stream in the input context");

        AVCodec* inputCodec = ffmpeg.avcodec_find_decoder(inputVideoStream->codecpar->codec_id);
        if (inputCodec == null)
            throw new ApplicationException("Failed to find the decoder");

        AVCodecContext* inputCodecContext = ffmpeg.avcodec_alloc_context3(inputCodec);
        ffmpeg.avcodec_parameters_to_context(inputCodecContext, inputVideoStream->codecpar);
        ffmpeg.avcodec_open2(inputCodecContext, inputCodec, null);

        AVFormatContext* outputFormatContext = null;
        ffmpeg.avformat_alloc_output_context2(&outputFormatContext, null, "mp4", outputFileName);
        if (outputFormatContext == null)
            throw new ApplicationException("Could not create output context");

        AVStream* outputVideoStream = ffmpeg.avformat_new_stream(outputFormatContext, inputCodec);
        if (outputVideoStream == null)
            throw new ApplicationException("Failed to create a video stream in the output context");

        ffmpeg.avcodec_parameters_copy(outputVideoStream->codecpar, inputVideoStream->codecpar);
        outputVideoStream->codecpar->codec_tag = 0;

        if (ffmpeg.avio_open(&outputFormatContext->pb, outputFileName, ffmpeg.AVIO_FLAG_WRITE) < 0)
            throw new ApplicationException("Failed to open the output file");

        if (ffmpeg.avformat_write_header(outputFormatContext, null) < 0)
            throw new ApplicationException("Error occurred when writing header to output file");

        AVPacket* pkt = ffmpeg.av_packet_alloc();

        while (ffmpeg.av_read_frame(inputFormatContext, pkt) >= 0)
        {
            if (pkt->stream_index == inputVideoStream->index)
            {
                ffmpeg.av_packet_rescale_ts(pkt, inputVideoStream->time_base, outputVideoStream->time_base);
                ffmpeg.av_interleaved_write_frame(outputFormatContext, pkt);
                ffmpeg.av_packet_unref(pkt);
            }
        }

        ffmpeg.av_write_trailer(outputFormatContext);
        ffmpeg.avcodec_close(inputCodecContext);
        ffmpeg.avformat_close_input(&inputFormatContext);
        ffmpeg.avio_closep(&outputFormatContext->pb);
        ffmpeg.avformat_free_context(outputFormatContext);
        ffmpeg.av_packet_free(&pkt);
    }
}