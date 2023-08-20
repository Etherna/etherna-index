using Etherna.EthernaIndex.Domain.Models.UserAgg;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Etherna.EthernaIndex.Domain.Models
{
    public class CommentTest
    {
        // Fields.
        private readonly string address = "0x300a31dBAB42863F4b0bEa3E03d0aa89D47DB3f0";
        private readonly User author;
        private readonly Mock<UserSharedInfo> userSharedInfoMock = new();
        private readonly Video video;

        // Constructors.
        public CommentTest()
        {
            userSharedInfoMock.Setup(s => s.EtherAddress).Returns(address);
            author = new User(userSharedInfoMock.Object);
            video = new Video(author);
        }

        // Tests.
        [Fact]
        public void Create_Comment()
        {
            // Arrange.
            var textComment = "first comment";

            // Action.
            var comment = new Comment(author, textComment, video);


            // Assert.
            Assert.False(comment.IsFrozen);
            Assert.Equal(0, comment.EditTimes);
            Assert.True(comment.IsEditable);
            Assert.Equal(textComment, comment.Text);
            Assert.Empty(comment.History);
        }

        [Fact]
        public void UpdateCommentShouldBeAddHistory()
        {
            // Arrange.
            var textComment = "first comment";
            var comment = new Comment(author, textComment, video);
            var secondComment = "another comment";


            // Action.
            comment.UpdateComment(secondComment);


            // Assert.
            Assert.False(comment.IsFrozen);
            Assert.Equal(1, comment.EditTimes);
            Assert.True(comment.IsEditable);
            Assert.Equal(secondComment, comment.Text);
            Assert.Contains(comment.History.Values, v => v == textComment);
        }

        [Fact]
        public void UpdateCommentShouldBeUseOnlyTenTimes()
        {
            // Arrange.
            var textComment = "first comment";
            var comment = new Comment(author, textComment, video);
            var secondComment = "another comment";
            for (var i = 1; i <= 10; i++)
                comment.UpdateComment($"{i}");


            // Action.
            Assert.Throws<InvalidOperationException>(() => comment.UpdateComment(secondComment));


            // Assert.
            Assert.False(comment.IsFrozen);
            Assert.Equal(10, comment.EditTimes);
            Assert.False(comment.IsEditable);
            Assert.Equal("10", comment.Text);
        }

        [Fact]
        public void SetAsDeletedByAuthor()
        {
            // Arrange.
            var textComment = "first comment";
            var comment = new Comment(author, textComment, video);


            // Action.
            comment.SetAsDeletedByAuthor();


            // Assert.
            Assert.True(comment.IsFrozen);
            Assert.Equal(0, comment.EditTimes);
            Assert.False(comment.IsEditable);
            Assert.Equal("(removed by author)", comment.Text);
            Assert.Contains(comment.History.Values, v => v == textComment);
        }

        [Fact]
        public void SetAsDeletedByModerator()
        {
            // Arrange.
            var textComment = "first comment";
            var comment = new Comment(author, textComment, video);
            string address = "0x500a31dBAB42863F4b0bEa3E03d0aa89D47DB3f0";
            Mock<UserSharedInfo> moderatorSharedInfoMock = new();
            moderatorSharedInfoMock.Setup(s => s.EtherAddress).Returns(address);
            var moderator = new User(moderatorSharedInfoMock.Object);


            // Action.
            comment.SetAsDeletedByModerator(moderator);


            // Assert.
            Assert.True(comment.IsFrozen);
            Assert.Equal(0, comment.EditTimes);
            Assert.False(comment.IsEditable);
            Assert.Equal("(removed by moderator)", comment.Text);
            Assert.Empty(comment.History);
        }

    }
}
