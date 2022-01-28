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
        private static readonly Action<ILogger, string, string, Exception> _createdCommentVideo =
            LoggerMessage.Define<string, string>(
                LogLevel.Information,
                new EventId(1, nameof(CreatedCommentVideo)),
                "User Id '{Id}' created new comment for video with hash {ManifestHash}");

        private static readonly Action<ILogger, string, string, Exception> _createdVideo =
            LoggerMessage.Define<string, string>(
                LogLevel.Information,
                new EventId(2, nameof(CreatedVideo)),
                "User Id '{Id}' created new video with hash {ManifestHash}");

        private static readonly Action<ILogger, string, Exception> _deletedVideo =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(3, nameof(DeletedVideo)),
                "Video with hash {ManifestHash} deleted by author");


        private static readonly Action<ILogger, string, string, Exception> _updatedVideo =
            LoggerMessage.Define<string, string>(
                LogLevel.Information,
                new EventId(5, nameof(CreatedVideo)),
                "Video hash {OldHash} updated with {NewHash} by author");

        private static readonly Action<ILogger, string, string, Exception> _votedVideo =
            LoggerMessage.Define<string, string>(
                LogLevel.Information,
                new EventId(4, nameof(CreatedCommentVideo)),
                "User Id '{Id}' voted video with hash {ManifestHash}");

        //*** WARNING LOGS ***

        //*** ERROR LOGS ***
        private static readonly Action<ILogger, string, Exception> _requestThrowedError =
            LoggerMessage.Define<string>(
                LogLevel.Error,
                new EventId(0, nameof(RequestThrowedError)),
                "Request {RequestId} throwed error");

        // Methods.
        public static void CreatedCommentVideo(this ILogger logger, string id, string manifestHash) =>
            _createdCommentVideo(logger, id, manifestHash, null!);

        public static void CreatedVideo(this ILogger logger, string id, string manifestHash) =>
            _createdVideo(logger, id, manifestHash, null!);

        public static void DeletedVideo(this ILogger logger, string manifestHash) =>
            _deletedVideo(logger, manifestHash, null!);

        public static void RequestThrowedError(this ILogger logger, string requestId) =>
            _requestThrowedError(logger, requestId, null!);

        public static void UpdatedVideo(this ILogger logger, string oldHash, string newHash) =>
            _updatedVideo(logger, oldHash, newHash, null!);

        public static void VotedVideo(this ILogger logger, string id, string manifestHash) =>
            _votedVideo(logger, id, manifestHash, null!);

    }
}
