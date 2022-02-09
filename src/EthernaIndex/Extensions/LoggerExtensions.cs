using Microsoft.Extensions.Logging;
using System;

namespace Etherna.EthernaIndex.Extensions
{
    /*
     * Always group similar log delegates by type, always use incremental event ids.
     * Last event id is: 5
     */
    public static class LoggerExtensions
    {
        // Fields.
        //*** DEBUG LOGS ***

        //*** INFORMATION LOGS ***
        private static readonly Action<ILogger, string, Exception> _authorDeletedVideo =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(3, nameof(AuthorDeletedVideo)),
                "Video with Id {VideoId} deleted by author");

        private static readonly Action<ILogger, string, string, Exception> _createdCommentVideo =
            LoggerMessage.Define<string, string>(
                LogLevel.Information,
                new EventId(1, nameof(CreatedCommentVideo)),
                "User Id '{UserId}' created new comment for video with Id {VideoId}");

        private static readonly Action<ILogger, string, string, Exception> _createdVideo =
            LoggerMessage.Define<string, string>(
                LogLevel.Information,
                new EventId(2, nameof(CreatedVideo)),
                "User Id '{UserId}' created new video with Id {VideoId}");

        private static readonly Action<ILogger, string, string, Exception> _updatedVideo =
            LoggerMessage.Define<string, string>(
                LogLevel.Information,
                new EventId(5, nameof(CreatedVideo)),
                "Video Id {VideoId} updated with {NewHash} by author");

        private static readonly Action<ILogger, string, string, Exception> _votedVideo =
            LoggerMessage.Define<string, string>(
                LogLevel.Information,
                new EventId(4, nameof(CreatedCommentVideo)),
                "User Id '{UserId}' voted video with hash {VideoId}");

        //*** WARNING LOGS ***

        //*** ERROR LOGS ***
        private static readonly Action<ILogger, string, Exception> _requestThrowedError =
            LoggerMessage.Define<string>(
                LogLevel.Error,
                new EventId(0, nameof(RequestThrowedError)),
                "Request {RequestId} throwed error");

        // Methods.
        public static void AuthorDeletedVideo(this ILogger logger, string videoId) =>
            _authorDeletedVideo(logger, videoId, null!);

        public static void CreatedCommentVideo(this ILogger logger, string userId, string videoId) =>
            _createdCommentVideo(logger, userId, videoId, null!);

        public static void CreatedVideo(this ILogger logger, string userId, string videoId) =>
            _createdVideo(logger, userId, videoId, null!);

        public static void RequestThrowedError(this ILogger logger, string requestId) =>
            _requestThrowedError(logger, requestId, null!);

        public static void UpdatedVideo(this ILogger logger, string videoId, string newHash) =>
            _updatedVideo(logger, videoId, newHash, null!);

        public static void VotedVideo(this ILogger logger, string userId, string videoId) =>
            _votedVideo(logger, userId, videoId, null!);

    }
}
