//   Copyright 2021-present Etherna Sagl
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using Microsoft.Extensions.Logging;
using System;

namespace Etherna.EthernaIndex.Services.Extensions
{
    /*
     * Always group similar log delegates by type, always use incremental event ids.
     * Last event id is: 10
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
                new EventId(5, nameof(UpdatedVideo)),
                "Video Id {VideoId} updated with {NewHash} by author");

        private static readonly Action<ILogger, string, string, Exception> _videoManifestValidationRetrievedManifest =
            LoggerMessage.Define<string, string>(
                LogLevel.Information,
                new EventId(7, nameof(VideoManifestValidationRetrievedManifest)),
                "Validation of video Id {VideoId} with manifest {ManifestHash} retrieved manifest");

        private static readonly Action<ILogger, string, string, Exception> _videoManifestValidationStarted =
            LoggerMessage.Define<string, string>(
                LogLevel.Information,
                new EventId(6, nameof(VideoManifestValidationStarted)),
                "Validation of video Id {VideoId} with manifest {ManifestHash} started");

        private static readonly Action<ILogger, string, string, Exception> _videoManifestValidationSucceeded =
            LoggerMessage.Define<string, string>(
                LogLevel.Information,
                new EventId(10, nameof(VideoManifestValidationSucceeded)),
                "Validation of video Id {VideoId} with manifest {ManifestHash} succeeded");

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

        private static readonly Action<ILogger, string, string, Exception> _videoManifestValidationCantRetrieveManifest =
            LoggerMessage.Define<string, string>(
                LogLevel.Error,
                new EventId(8, nameof(VideoManifestValidationCantRetrieveManifest)),
                "Validation of video Id {VideoId} with manifest {ManifestHash} can't retrie manifest");

        private static readonly Action<ILogger, string, string, Exception> _videoManifestValidationFailedWithErrors =
            LoggerMessage.Define<string, string>(
                LogLevel.Error,
                new EventId(9, nameof(VideoManifestValidationFailedWithErrors)),
                "Validation of video Id {VideoId} with manifest {ManifestHash} failed with errors");

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

        public static void VideoManifestValidationCantRetrieveManifest(this ILogger logger, string videoId, string manifestHash, Exception? exception) =>
            _videoManifestValidationCantRetrieveManifest(logger, videoId, manifestHash, exception!);

        public static void VideoManifestValidationFailedWithErrors(this ILogger logger, string videoId, string manifestHash, Exception? exception) =>
            _videoManifestValidationFailedWithErrors(logger, videoId, manifestHash, exception!);

        public static void VideoManifestValidationRetrievedManifest(this ILogger logger, string videoId, string manifestHash) =>
            _videoManifestValidationRetrievedManifest(logger, videoId, manifestHash, null!);

        public static void VideoManifestValidationStarted(this ILogger logger, string videoId, string manifestHash) =>
            _videoManifestValidationStarted(logger, videoId, manifestHash, null!);

        public static void VideoManifestValidationSucceeded(this ILogger logger, string videoId, string manifestHash) =>
            _videoManifestValidationSucceeded(logger, videoId, manifestHash, null!);

        public static void VotedVideo(this ILogger logger, string userId, string videoId) =>
            _votedVideo(logger, userId, videoId, null!);

    }
}
