using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using Xunit;

namespace Etherna.EthernaIndex.Domain.Models
{
    public class CommentTest
    {
        // Fields.
        private readonly Mock<User> authorMock = new();
        private readonly Mock<Video> videoMock = new();

        // Tests.
        [Fact]
        public void CreateComment()
        {
            // Arrange.
            var textComment = "first comment";

            // Action.
            var beforeTime = DateTime.UtcNow;
            var comment = new Comment(authorMock.Object, textComment, videoMock.Object);
            var afterTime = DateTime.UtcNow;

            // Assert.
            Assert.Equal(authorMock.Object, comment.Author);
            Assert.True(comment.IsEditable);
            Assert.False(comment.IsFrozen);
            Assert.Equal(textComment, comment.LastText);
            Assert.InRange(comment.LastUpdateDateTime, beforeTime, afterTime);
            Assert.Single(comment.TextHistory, KeyValuePair.Create(comment.LastUpdateDateTime, comment.LastText));
            Assert.Equal(videoMock.Object, comment.Video);
        }

        [Fact]
        public void EditCommentShouldAddHistory()
        {
            // Arrange.
            var firstComment = "first comment";
            var comment = new Comment(authorMock.Object, firstComment, videoMock.Object);
            var firstCommentDateTime = comment.LastUpdateDateTime;
            var secondComment = "another comment";
        
            // Action.
            var beforeTime = DateTime.UtcNow;
            comment.EditByAuthor(secondComment);
            var afterTime = DateTime.UtcNow;
        
            // Assert.
            Assert.Equal(authorMock.Object, comment.Author);
            Assert.True(comment.IsEditable);
            Assert.False(comment.IsFrozen);
            Assert.Equal(secondComment, comment.LastText);
            Assert.InRange(comment.LastUpdateDateTime, beforeTime, afterTime);
            Assert.Equal(comment.TextHistory, new[]
            {
                KeyValuePair.Create(firstCommentDateTime, firstComment),
                KeyValuePair.Create(comment.LastUpdateDateTime, comment.LastText)
            });
            Assert.Equal(videoMock.Object, comment.Video);
        }
        
        [Fact]
        public void EditCommentShouldBeLimitedInTimes()
        {
            // Arrange.
            var firstComment = "first comment";
            var comment = new Comment(authorMock.Object, firstComment, videoMock.Object);
            for (var i = 2; i <= Comment.MaxEditHistory; i++)
                comment.EditByAuthor($"{i}");
            var lastComment = "another comment";
        
            // Action.
            Assert.Throws<InvalidOperationException>(() => comment.EditByAuthor(lastComment));
        
            // Assert.
            Assert.Equal(authorMock.Object, comment.Author);
            Assert.False(comment.IsEditable);
            Assert.False(comment.IsFrozen);
            Assert.Equal(Comment.MaxEditHistory.ToString(CultureInfo.InvariantCulture), comment.LastText);
            Assert.Equal(Comment.MaxEditHistory, comment.TextHistory.Count);
            Assert.Equal(videoMock.Object, comment.Video);
        }
        
        [Fact]
        public void SetAsDeletedByAuthor()
        {
            // Arrange.
            var firstComment = "first comment";
            var secondComment = "second comment";
            var comment = new Comment(authorMock.Object, firstComment, videoMock.Object);
            comment.EditByAuthor(secondComment);
        
            // Action.
            var beforeTime = DateTime.UtcNow;
            comment.SetAsDeletedByAuthor();
            var afterTime = DateTime.UtcNow;
        
            // Assert.
            Assert.Equal(authorMock.Object, comment.Author);
            Assert.False(comment.IsEditable);
            Assert.True(comment.IsFrozen);
            Assert.Equal(Comment.RemovedByAuthorReplaceText, comment.LastText);
            Assert.InRange(comment.LastUpdateDateTime, beforeTime, afterTime);
            Assert.Single(comment.TextHistory, KeyValuePair.Create(comment.LastUpdateDateTime, comment.LastText));
            Assert.Equal(videoMock.Object, comment.Video);
        }
        
        [Fact]
        public void SetAsDeletedByModerator()
        {
            // Arrange.
            var firstComment = "first comment";
            var secondComment = "second comment";
            var comment = new Comment(authorMock.Object, firstComment, videoMock.Object);
            comment.EditByAuthor(secondComment);
        
            // Action.
            var beforeTime = DateTime.UtcNow;
            comment.SetAsDeletedByModerator();
            var afterTime = DateTime.UtcNow;
        
            // Assert.
            Assert.Equal(authorMock.Object, comment.Author);
            Assert.False(comment.IsEditable);
            Assert.True(comment.IsFrozen);
            Assert.Equal(Comment.RemovedByModeratorReplaceText, comment.LastText);
            Assert.InRange(comment.LastUpdateDateTime, beforeTime, afterTime);
            Assert.Single(comment.TextHistory, KeyValuePair.Create(comment.LastUpdateDateTime, comment.LastText));
            Assert.Equal(videoMock.Object, comment.Video);
        }
    }
}
