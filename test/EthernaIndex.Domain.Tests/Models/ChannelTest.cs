using System;
using Xunit;

namespace Etherna.EthernaIndex.Domain.Models
{
    public class ChannelTest
    {
        // Consts.
        private const string ChannelAddress = "ImAnAddress";
        private const string VideoHash = "ImAnHash";

        // Fields.
        private readonly Channel channel;
        private readonly Video sampleVideo;

        // Constructors.
        public ChannelTest()
        {
            channel = new Channel(ChannelAddress);
            sampleVideo = new Video("", TimeSpan.FromMinutes(10), channel, "", "title", VideoHash);
        }

        // Tests.
        [Fact]
        public void AddVideo()
        {
            // Action.
            channel.AddVideo(sampleVideo);

            // Assert.
            Assert.Contains(sampleVideo, channel.Videos);
            Assert.Equal(channel, sampleVideo.OwnerChannel);
        }

        [Fact]
        public void RemoveVideo()
        {
            // Setup.
            channel.AddVideo(sampleVideo);

            // Action.
            channel.RemoveVideo(sampleVideo);

            // Assert.
            Assert.DoesNotContain(sampleVideo, channel.Videos);
        }
    }
}
