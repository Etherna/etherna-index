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

        private static readonly Action<ILogger, string, Exception> _deletedCommentVideoByOwner =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(2, nameof(DeletedCommentVideoByOwner)),
                "Comment Id {CommentId} deleted by owner");

        private static readonly Action<ILogger, int, int, Exception> _lastUploadedVideos =
            LoggerMessage.Define<int, int>(
                LogLevel.Information,
                new EventId(4, nameof(LastUploadedVideos)),
                "Last uploaded video paginated Page: {Page} Take: {Take}");

        private static readonly Action<ILogger, string, Exception> _manifestFindByHash =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(4, nameof(ManifestFindByHash)),
                "Find video by Manifest Hash {ManifestHash}");

        private static readonly Action<ILogger, string, Exception> _moderateComment =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(4, nameof(ModerateComment)),
                "Comment id: {CommentId} moderated");

        private static readonly Action<ILogger, string, Exception> _moderateVideo =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(4, nameof(ModerateVideo)),
                "Video id: {CommentId} moderated");

        private static readonly Action<ILogger, string, string, Exception> _reportVideoChangeDescription =
            LoggerMessage.Define<string, string>(
                LogLevel.Information,
                new EventId(4, nameof(ReportVideoChangeDescription)),
                "Change reported description for video id {VideoId} with Manifest Hash {ManifestHash}");

        private static readonly Action<ILogger, string, string, Exception> _reportVideoCreate =
            LoggerMessage.Define<string, string>(
                LogLevel.Information,
                new EventId(4, nameof(ReportVideoCreate)),
                "Reported video id {VideoId} with Manifest Hash {ManifestHash}");

        private static readonly Action<ILogger, string, string, Exception> _updatedVideo =
            LoggerMessage.Define<string, string>(
                LogLevel.Information,
                new EventId(5, nameof(UpdatedVideo)),
                "Video Id {VideoId} updated with {NewHash} by author");

        private static readonly Action<ILogger, int, int, Exception> _userListPaginated =
            LoggerMessage.Define<int, int>(
                LogLevel.Information,
                new EventId(5, nameof(UserListPaginated)),
                "Get users  paginated Page: {Page} Take: {Take}");

        private static readonly Action<ILogger, string, Exception> _userFindByAddress =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(5, nameof(UserFindByAddress)),
                "User find with address {Address}");

        private static readonly Action<ILogger, string, Exception> _userGetCurrent =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(5, nameof(UserGetCurrent)),
                "Get current user with address {Address}");

        private static readonly Action<ILogger, string, int, int, Exception> _userGetVideoPaginated =
            LoggerMessage.Define<string, int, int>(
                LogLevel.Information,
                new EventId(5, nameof(UserGetVideoPaginated)),
                "Get video for user address {Address} paginated Page: {Page} Take: {Take}");

        private static readonly Action<ILogger, string, Exception> _videoFindById =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(4, nameof(VideoFindById)),
                "Find video by Id {VideoId}");

        private static readonly Action<ILogger, string, int, int, Exception> _videoGetComments =
            LoggerMessage.Define<string, int, int>(
                LogLevel.Information,
                new EventId(4, nameof(VideoGetComments)),
                "Get comments from video id {VideoId} paginated Page: {Page} Take: {Take}");

        private static readonly Action<ILogger, string, Exception> _videoGetValidationStatusById =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(4, nameof(VideoGetValidationStatusById)),
                "Get validation status by video id {VideoId}");

        private static readonly Action<ILogger, string, Exception> _videoGetValidationStatusByHash =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(4, nameof(VideoGetValidationStatusByHash)),
                "Get validation status by manifest hash {ManifestHash}");

        private static readonly Action<ILogger, string, string, Exception> _votedVideo =
            LoggerMessage.Define<string, string>(
                LogLevel.Information,
                new EventId(4, nameof(VotedVideo)),
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

        public static void DeletedCommentVideoByOwner(this ILogger logger, string commentId) =>
            _deletedCommentVideoByOwner(logger, commentId, null!);

        public static void LastUploadedVideos(this ILogger logger, int page, int take) =>
             _lastUploadedVideos(logger, page, take, null!);

        public static void ManifestFindByHash(this ILogger logger, string manifestHash) =>
            _manifestFindByHash(logger, manifestHash, null!);

        public static void ModerateComment(this ILogger logger, string commentId) =>
            _moderateComment(logger, commentId, null!);

        public static void ModerateVideo(this ILogger logger, string videoId) =>
            _moderateVideo(logger, videoId, null!);

        public static void ReportVideoCreate(this ILogger logger, string videoId, string manifestHash) =>
            _reportVideoCreate(logger, videoId, manifestHash, null!);

        public static void ReportVideoChangeDescription(this ILogger logger, string videoId, string manifestHash) =>
            _reportVideoChangeDescription(logger, videoId, manifestHash, null!);

        public static void RequestThrowedError(this ILogger logger, string requestId) =>
            _requestThrowedError(logger, requestId, null!);

        public static void UpdatedVideo(this ILogger logger, string videoId, string newHash) =>
            _updatedVideo(logger, videoId, newHash, null!);

        public static void UserFindByAddress(this ILogger logger, string address) =>
            _userFindByAddress(logger, address, null!);

        public static void UserGetCurrent(this ILogger logger, string address) =>
            _userGetCurrent(logger, address, null!);

        public static void UserListPaginated(this ILogger logger, int page, int take) =>
            _userListPaginated(logger, page, take, null!);

        public static void UserGetVideoPaginated(this ILogger logger, string address, int page, int take) =>
            _userGetVideoPaginated(logger, address, page, take, null!);

        public static void VideoFindById(this ILogger logger, string videoId) =>
            _videoFindById(logger, videoId, null!);

        public static void VideoGetComments(this ILogger logger, string videoId, int page, int take) =>
            _videoGetComments(logger, videoId, page, take, null!);

        public static void VideoGetValidationStatusById(this ILogger logger, string videoId) =>
            _videoGetValidationStatusById(logger, videoId, null!);

        public static void VideoGetValidationStatusByHash(this ILogger logger, string manifestHash) =>
             _videoGetValidationStatusByHash(logger, manifestHash, null!);

        public static void VotedVideo(this ILogger logger, string userId, string videoId) =>
            _votedVideo(logger, userId, videoId, null!);
    }
}
