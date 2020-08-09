using Etherna.EthernaIndex.Domain.Models.Swarm;
using System;
using Xunit;

namespace Etherna.EthernaIndex.Domain.Models
{
    public class UserTest
    {
        // Consts.
        private const string UserAddress = "0xFb6916095cA1Df60bb79ce92cE3EA74c37C5d359";
        private readonly SwarmContentHash VideoManifestHash = new SwarmContentHash("33f1ea45b3404d1691911729a5dd618216bbd2031c9bf1459d4f4542fb13e067");

        // Fields.
        private readonly User user;
        private readonly Video video;

        // Constructors.
        public UserTest()
        {
            user = new User(UserAddress);
            video = new Video(null, EncryptionType.Plain, VideoManifestHash, user);
        }

        // Tests.
        [Fact]
        public void AddVideo()
        {
            // Action.
            user.AddVideo(video);

            // Assert.
            Assert.Contains(video, user.Videos);
            Assert.Equal(user, video.Owner);
        }

        [Fact]
        public void InvalidAddress()
        {
            // Assert.
            Assert.Throws<ArgumentNullException>(() => new User(null));
            Assert.Throws<ArgumentException>(() => new User("ImNotAnAddress"));
        }

        [Fact]
        public void RemoveVideo()
        {
            // Setup.
            user.AddVideo(video);

            // Action.
            user.RemoveVideo(video);

            // Assert.
            Assert.DoesNotContain(video, user.Videos);
        }
    }
}
