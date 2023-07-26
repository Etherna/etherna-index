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
using System.Collections.Generic;

namespace Etherna.EthernaIndex.Services.Extensions
{
    /*
     * Always group similar log delegates by type, always use incremental event ids.
     * Last event id is: 28
     */
    public static class LoggerExtensions
    {
        // Fields.
        //*** DEBUG LOGS ***
        private static readonly Action<ILogger, string, string, Exception> _videoManifestValidationRetrievedManifest =
            LoggerMessage.Define<string, string>(
                LogLevel.Debug,
                new EventId(7, nameof(VideoManifestValidationRetrievedManifest)),
                "Validation of video Id {VideoId} with manifest {ManifestHash} retrieved manifest");

        private static readonly Action<ILogger, string, string, Exception> _videoManifestValidationStarted =
            LoggerMessage.Define<string, string>(
                LogLevel.Debug,
                new EventId(6, nameof(VideoManifestValidationStarted)),
                "Validation of video Id {VideoId} with manifest {ManifestHash} started");

        //*** INFORMATION LOGS ***
        private static readonly Action<ILogger, string, Exception> _authorDeleteVideo =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(3, nameof(AuthorDeleteVideo)),
                "Video with Id {VideoId} deleted by author");

        private static readonly Action<ILogger, string, string, Exception> _changeVideoReportDescription =
            LoggerMessage.Define<string, string>(
                LogLevel.Information,
                new EventId(24, nameof(ChangeVideoReportDescription)),
                "Change reported description for video id {VideoId} with Manifest Hash {ManifestHash}");

        private static readonly Action<ILogger, string, string, Exception> _createVideoComment =
            LoggerMessage.Define<string, string>(
                LogLevel.Information,
                new EventId(1, nameof(CreateVideoComment)),
                "User Id '{UserId}' created new comment for video with Id {VideoId}");

        private static readonly Action<ILogger, string, string, Exception> _createVideoReport =
            LoggerMessage.Define<string, string>(
                LogLevel.Information,
                new EventId(25, nameof(CreateVideoReport)),
                "Reported video id {VideoId} with Manifest Hash {ManifestHash}");

        private static readonly Action<ILogger, string, Exception> _findManifestByHash =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(21, nameof(FindManifestByHash)),
                "Find video by Manifest Hash {ManifestHash}");

        private static readonly Action<ILogger, string, Exception> _findUserByAddress =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(12, nameof(FindUserByAddress)),
                "User find with address {Address}");

        private static readonly Action<ILogger, string, Exception> _findVideoById =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(15, nameof(FindVideoById)),
                "Find video by Id {VideoId}");

        private static readonly Action<ILogger, string, string, IEnumerable<string>, Exception> _forcedVideoManifestsValidation =
            LoggerMessage.Define<string, string, IEnumerable<string>>(
                LogLevel.Information,
                new EventId(28, nameof(ForcedVideoManifestsValidation)),
                "User {UserId} forced validation of video {VideoId} on manifests {ManifestHashes}");

        private static readonly Action<ILogger, IEnumerable<string>, Exception> _getBulkVideoManifestValidationStatusByHashes =
            LoggerMessage.Define<IEnumerable<string>>(
                LogLevel.Information,
                new EventId(27, nameof(GetBulkVideoManifestValidationStatusByHashes)),
                "Get bulk validation status by video manifests hashes {ManifestHashes}");

        private static readonly Action<ILogger, IEnumerable<string>, Exception> _getBulkVideoValidationStatusByIds =
            LoggerMessage.Define<IEnumerable<string>>(
                LogLevel.Information,
                new EventId(26, nameof(GetBulkVideoValidationStatusByIds)),
                "Get bulk validation status by videos ids {VideoIds}");

        private static readonly Action<ILogger, string, Exception> _getCurrentUser =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(13, nameof(GetCurrentUser)),
                "Get current user with address {Address}");

        private static readonly Action<ILogger, int, int, Exception> _getLastUploadedVideos =
            LoggerMessage.Define<int, int>(
                LogLevel.Information,
                new EventId(20, nameof(GetLastUploadedVideos)),
                "Last uploaded video paginated Page: {Page} Take: {Take}");

        private static readonly Action<ILogger, int, int, Exception> _getUserListPaginated =
            LoggerMessage.Define<int, int>(
                LogLevel.Information,
                new EventId(11, nameof(GetUserListPaginated)),
                "Get users paginated Page: {Page} Take: {Take}");

        private static readonly Action<ILogger, string, int, int, Exception> _getUserVideosPaginated =
            LoggerMessage.Define<string, int, int>(
                LogLevel.Information,
                new EventId(14, nameof(GetUserVideosPaginated)),
                "Get video for user address {Address} paginated Page: {Page} Take: {Take}");

        private static readonly Action<ILogger, string, int, int, Exception> _getVideoComments =
            LoggerMessage.Define<string, int, int>(
                LogLevel.Information,
                new EventId(16, nameof(GetVideoComments)),
                "Get comments from video id {VideoId} paginated Page: {Page} Take: {Take}");

        private static readonly Action<ILogger, string, Exception> _getVideoManifestValidationStatusByHash =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(18, nameof(GetVideoManifestValidationStatusByHash)),
                "Get validation status by manifest hash {ManifestHash}");

        private static readonly Action<ILogger, string, Exception> _getVideoValidationStatusById =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(17, nameof(GetVideoValidationStatusById)),
                "Get validation status by video id {VideoId}");

        private static readonly Action<ILogger, string, Exception> _moderateComment =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(22, nameof(ModerateComment)),
                "Comment id: {CommentId} moderated");

        private static readonly Action<ILogger, string, Exception> _moderateVideo =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(23, nameof(ModerateVideo)),
                "Video id: {CommentId} moderated");

        private static readonly Action<ILogger, string, Exception> _ownerDeleteVideoComment =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(19, nameof(OwnerDeleteVideoComment)),
                "Comment Id {CommentId} deleted by owner");

        private static readonly Action<ILogger, string, string, Exception> _updateVideo =
            LoggerMessage.Define<string, string>(
                LogLevel.Information,
                new EventId(5, nameof(UpdateVideo)),
                "Video Id {VideoId} updated with manifest {NewHash} by author");

        private static readonly Action<ILogger, string, string, Exception> _videoCreated =
            LoggerMessage.Define<string, string>(
                LogLevel.Information,
                new EventId(2, nameof(VideoCreated)),
                "User Id '{UserId}' created new video with Id {VideoId}");

        private static readonly Action<ILogger, string, string, Exception> _videoManifestValidationSucceeded =
            LoggerMessage.Define<string, string>(
                LogLevel.Information,
                new EventId(10, nameof(VideoManifestValidationSucceeded)),
                "Validation of video Id {VideoId} with manifest {ManifestHash} succeeded");

        private static readonly Action<ILogger, string, string, Exception> _videoVoted =
            LoggerMessage.Define<string, string>(
                LogLevel.Information,
                new EventId(4, nameof(VideoVoted)),
                "User Id '{UserId}' voted video with Id {VideoId}");

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
        public static void AuthorDeleteVideo(this ILogger logger, string videoId) =>
            _authorDeleteVideo(logger, videoId, null!);

        public static void ChangeVideoReportDescription(this ILogger logger, string videoId, string manifestHash) =>
            _changeVideoReportDescription(logger, videoId, manifestHash, null!);

        public static void CreateVideoComment(this ILogger logger, string userId, string videoId) =>
            _createVideoComment(logger, userId, videoId, null!);

        public static void CreateVideoReport(this ILogger logger, string videoId, string manifestHash) =>
            _createVideoReport(logger, videoId, manifestHash, null!);

        public static void FindManifestByHash(this ILogger logger, string manifestHash) =>
            _findManifestByHash(logger, manifestHash, null!);

        public static void FindUserByAddress(this ILogger logger, string address) =>
            _findUserByAddress(logger, address, null!);

        public static void FindVideoById(this ILogger logger, string videoId) =>
            _findVideoById(logger, videoId, null!);

        public static void ForcedVideoManifestsValidation(this ILogger logger, string userId, string videoid, IEnumerable<string> manifestHashes) =>
            _forcedVideoManifestsValidation(logger, userId, videoid, manifestHashes, null!);

        public static void GetBulkVideoManifestValidationStatusByHashes(this ILogger logger, IEnumerable<string> manifestHashes) =>
            _getBulkVideoManifestValidationStatusByHashes(logger, manifestHashes, null!);

        public static void GetBulkVideoValidationStatusByIds(this ILogger logger, IEnumerable<string> videoIds) =>
            _getBulkVideoValidationStatusByIds(logger, videoIds, null!);

        public static void GetCurrentUser(this ILogger logger, string address) =>
            _getCurrentUser(logger, address, null!);

        public static void GetLastUploadedVideos(this ILogger logger, int page, int take) =>
            _getLastUploadedVideos(logger, page, take, null!);

        public static void GetUserListPaginated(this ILogger logger, int page, int take) =>
            _getUserListPaginated(logger, page, take, null!);

        public static void GetUserVideosPaginated(this ILogger logger, string address, int page, int take) =>
            _getUserVideosPaginated(logger, address, page, take, null!);

        public static void GetVideoComments(this ILogger logger, string videoId, int page, int take) =>
            _getVideoComments(logger, videoId, page, take, null!);

        public static void GetVideoManifestValidationStatusByHash(this ILogger logger, string manifestHash) =>
             _getVideoManifestValidationStatusByHash(logger, manifestHash, null!);

        public static void GetVideoValidationStatusById(this ILogger logger, string videoId) =>
            _getVideoValidationStatusById(logger, videoId, null!);

        public static void ModerateComment(this ILogger logger, string commentId) =>
            _moderateComment(logger, commentId, null!);

        public static void ModerateVideo(this ILogger logger, string videoId) =>
            _moderateVideo(logger, videoId, null!);

        public static void OwnerDeleteVideoComment(this ILogger logger, string commentId) =>
            _ownerDeleteVideoComment(logger, commentId, null!);

        public static void RequestThrowedError(this ILogger logger, string requestId) =>
            _requestThrowedError(logger, requestId, null!);

        public static void UpdateVideo(this ILogger logger, string videoId, string newHash) =>
            _updateVideo(logger, videoId, newHash, null!);

        public static void VideoCreated(this ILogger logger, string userId, string videoId) =>
            _videoCreated(logger, userId, videoId, null!);

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

        public static void VideoVoted(this ILogger logger, string userId, string videoId) =>
            _videoVoted(logger, userId, videoId, null!);
    }
}
