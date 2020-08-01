using Etherna.EthernaIndex.Domain.Models.Swarm;
using System;
using Xunit;

namespace Etherna.EthernaIndex.Domain.Models
{
    public class ChannelTest
    {
        // Consts.
        private const string ChannelAddress = "0xFb6916095cA1Df60bb79ce92cE3EA74c37C5d359";
        private readonly SwarmContentHash VideoHash = new SwarmContentHash("33f1ea45b3404d1691911729a5dd618216bbd2031c9bf1459d4f4542fb13e067", false);

        // Fields.
        private readonly Channel channel;
        private readonly Video sampleVideo;

        // Constructors.
        public ChannelTest()
        {
            channel = new Channel(ChannelAddress);
            sampleVideo = new Video("", null, EncryptionType.Plain, VideoHash, TimeSpan.FromMinutes(10), channel, null, "title");
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
        public void InvalidAddress()
        {
            // Assert.
            Assert.Throws<ArgumentNullException>(() => new Channel(null));
            Assert.Throws<ArgumentException>(() => new Channel("ImNotAnAddress"));
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
