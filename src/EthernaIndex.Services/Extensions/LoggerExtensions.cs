// Copyright 2021-present Etherna SA
// This file is part of Etherna Index.
// 
// Etherna Index is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// Etherna Index is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with Etherna Index.
// If not, see <https://www.gnu.org/licenses/>.

using Etherna.SwarmSdk.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Etherna.EthernaIndex.Services.Extensions
{
    /*
     * Always group similar log delegates by type, always use incremental event ids.
     * Last event id is: 29
     */
    public static class LoggerExtensions
    {
        // Fields.
        //*** DEBUG LOGS ***
        private static readonly Action<ILogger, string, SwarmReference, Exception> _videoManifestValidationRetrievedManifest =
            LoggerMessage.Define<string, SwarmReference>(
                LogLevel.Debug,
                new EventId(7, nameof(VideoManifestValidationRetrievedManifest)),
                "Validation of video Id {VideoId} with manifest {ManifestReference} retrieved manifest");

        private static readonly Action<ILogger, string, SwarmReference, Exception> _videoManifestValidationStarted =
            LoggerMessage.Define<string, SwarmReference>(
                LogLevel.Debug,
                new EventId(6, nameof(VideoManifestValidationStarted)),
                "Validation of video Id {VideoId} with manifest {ManifestReference} started");

        //*** INFORMATION LOGS ***
        private static readonly Action<ILogger, string, Exception> _authorDeleteVideo =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(3, nameof(AuthorDeleteVideo)),
                "Video with Id {VideoId} deleted by author");

        private static readonly Action<ILogger, string, SwarmReference, Exception> _changeVideoReportDescription =
            LoggerMessage.Define<string, SwarmReference>(
                LogLevel.Information,
                new EventId(24, nameof(ChangeVideoReportDescription)),
                "Change reported description for video id {VideoId} with Manifest Reference {ManifestReference}");

        private static readonly Action<ILogger, string, string, Exception> _createVideoComment =
            LoggerMessage.Define<string, string>(
                LogLevel.Information,
                new EventId(1, nameof(CreateVideoComment)),
                "User Id '{UserId}' created new comment for video with Id {VideoId}");

        private static readonly Action<ILogger, string, SwarmReference, Exception> _createVideoReport =
            LoggerMessage.Define<string, SwarmReference>(
                LogLevel.Information,
                new EventId(25, nameof(CreateVideoReport)),
                "Reported video id {VideoId} with Manifest Reference {ManifestReference}");

        private static readonly Action<ILogger, SwarmReference, Exception> _findManifestByReference =
            LoggerMessage.Define<SwarmReference>(
                LogLevel.Information,
                new EventId(21, nameof(FindManifestByReference)),
                "Find video by Manifest Reference {ManifestReference}");

        private static readonly Action<ILogger, EthAddress, Exception> _findUserByAddress =
            LoggerMessage.Define<EthAddress>(
                LogLevel.Information,
                new EventId(12, nameof(FindUserByAddress)),
                "User find with address {Address}");

        private static readonly Action<ILogger, string, Exception> _findVideoById =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(15, nameof(FindVideoById)),
                "Find video by Id {VideoId}");

        private static readonly Action<ILogger, string, string, IEnumerable<SwarmReference>, Exception> _forcedVideoManifestsValidation =
            LoggerMessage.Define<string, string, IEnumerable<SwarmReference>>(
                LogLevel.Information,
                new EventId(28, nameof(ForcedVideoManifestsValidation)),
                "User {UserId} forced validation of video {VideoId} on manifests {ManifestReferences}");

        private static readonly Action<ILogger, IEnumerable<SwarmReference>, Exception> _getBulkVideoManifestValidationStatusByReferences =
            LoggerMessage.Define<IEnumerable<SwarmReference>>(
                LogLevel.Information,
                new EventId(27, nameof(GetBulkVideoManifestValidationStatusByReferences)),
                "Get bulk validation status by video manifests references {ManifestReferences}");

        private static readonly Action<ILogger, IEnumerable<string>, Exception> _getBulkVideoValidationStatusByIds =
            LoggerMessage.Define<IEnumerable<string>>(
                LogLevel.Information,
                new EventId(26, nameof(GetBulkVideoValidationStatusByIds)),
                "Get bulk validation status by videos ids {VideoIds}");

        private static readonly Action<ILogger, EthAddress, Exception> _getCurrentUser =
            LoggerMessage.Define<EthAddress>(
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

        private static readonly Action<ILogger, EthAddress, int, int, Exception> _getUserVideosPaginated =
            LoggerMessage.Define<EthAddress, int, int>(
                LogLevel.Information,
                new EventId(14, nameof(GetUserVideosPaginated)),
                "Get video for user address {Address} paginated Page: {Page} Take: {Take}");

        private static readonly Action<ILogger, string, int, int, Exception> _getVideoComments =
            LoggerMessage.Define<string, int, int>(
                LogLevel.Information,
                new EventId(16, nameof(GetVideoComments)),
                "Get comments from video id {VideoId} paginated Page: {Page} Take: {Take}");

        private static readonly Action<ILogger, SwarmReference, Exception> _getVideoManifestValidationStatusByReference =
            LoggerMessage.Define<SwarmReference>(
                LogLevel.Information,
                new EventId(18, nameof(GetVideoManifestValidationStatusByReference)),
                "Get validation status by manifest reference {ManifestReference}");

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

        private static readonly Action<ILogger, string, Exception> _updatedComment =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(29, nameof(UpdatedComment)),
                "Comment Id {CommentId} updated  by author");

        private static readonly Action<ILogger, string, SwarmReference, Exception> _updatedVideo =
            LoggerMessage.Define<string, SwarmReference>(
                LogLevel.Information,
                new EventId(5, nameof(UpdatedVideo)),
                "Video Id {VideoId} updated with manifest {NewReference} by author");

        private static readonly Action<ILogger, string, string, Exception> _videoCreated =
            LoggerMessage.Define<string, string>(
                LogLevel.Information,
                new EventId(2, nameof(VideoCreated)),
                "User Id '{UserId}' created new video with Id {VideoId}");

        private static readonly Action<ILogger, string, SwarmReference, Exception> _videoManifestValidationSucceeded =
            LoggerMessage.Define<string, SwarmReference>(
                LogLevel.Information,
                new EventId(10, nameof(VideoManifestValidationSucceeded)),
                "Validation of video Id {VideoId} with manifest {ManifestReference} succeeded");

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

        private static readonly Action<ILogger, string, SwarmReference, Exception> _videoManifestValidationCantRetrieveManifest =
            LoggerMessage.Define<string, SwarmReference>(
                LogLevel.Error,
                new EventId(8, nameof(VideoManifestValidationCantRetrieveManifest)),
                "Validation of video Id {VideoId} with manifest {ManifestReference} can't retrie manifest");

        private static readonly Action<ILogger, string, SwarmReference, Exception> _videoManifestValidationFailedWithErrors =
            LoggerMessage.Define<string, SwarmReference>(
                LogLevel.Error,
                new EventId(9, nameof(VideoManifestValidationFailedWithErrors)),
                "Validation of video Id {VideoId} with manifest {ManifestReference} failed with errors");

        // Methods.
        public static void AuthorDeleteVideo(this ILogger logger, string videoId) =>
            _authorDeleteVideo(logger, videoId, null!);

        public static void ChangeVideoReportDescription(this ILogger logger, string videoId, SwarmReference manifestReference) =>
            _changeVideoReportDescription(logger, videoId, manifestReference, null!);

        public static void CreateVideoComment(this ILogger logger, string userId, string videoId) =>
            _createVideoComment(logger, userId, videoId, null!);

        public static void CreateVideoReport(this ILogger logger, string videoId, SwarmReference manifestReference) =>
            _createVideoReport(logger, videoId, manifestReference, null!);

        public static void FindManifestByReference(this ILogger logger, SwarmReference manifestReference) =>
            _findManifestByReference(logger, manifestReference, null!);

        public static void FindUserByAddress(this ILogger logger, EthAddress address) =>
            _findUserByAddress(logger, address, null!);

        public static void FindVideoById(this ILogger logger, string videoId) =>
            _findVideoById(logger, videoId, null!);

        public static void ForcedVideoManifestsValidation(this ILogger logger, string userId, string videoid, IEnumerable<SwarmReference> manifestReferences) =>
            _forcedVideoManifestsValidation(logger, userId, videoid, manifestReferences, null!);

        public static void GetBulkVideoManifestValidationStatusByReferences(this ILogger logger, IEnumerable<SwarmReference> manifestReferences) =>
            _getBulkVideoManifestValidationStatusByReferences(logger, manifestReferences, null!);

        public static void GetBulkVideoValidationStatusByIds(this ILogger logger, IEnumerable<string> videoIds) =>
            _getBulkVideoValidationStatusByIds(logger, videoIds, null!);

        public static void GetCurrentUser(this ILogger logger, EthAddress address) =>
            _getCurrentUser(logger, address, null!);

        public static void GetLastUploadedVideos(this ILogger logger, int page, int take) =>
            _getLastUploadedVideos(logger, page, take, null!);

        public static void GetUserListPaginated(this ILogger logger, int page, int take) =>
            _getUserListPaginated(logger, page, take, null!);

        public static void GetUserVideosPaginated(this ILogger logger, EthAddress address, int page, int take) =>
            _getUserVideosPaginated(logger, address, page, take, null!);

        public static void GetVideoComments(this ILogger logger, string videoId, int page, int take) =>
            _getVideoComments(logger, videoId, page, take, null!);

        public static void GetVideoManifestValidationStatusByReference(this ILogger logger, SwarmReference manifestReference) =>
             _getVideoManifestValidationStatusByReference(logger, manifestReference, null!);

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

        public static void UpdatedComment(this ILogger logger, string commentId) =>
            _updatedComment(logger, commentId, null!);

        public static void UpdatedVideo(this ILogger logger, string videoId, SwarmReference newReference) =>
            _updatedVideo(logger, videoId, newReference, null!);

        public static void VideoCreated(this ILogger logger, string userId, string videoId) =>
            _videoCreated(logger, userId, videoId, null!);

        public static void VideoManifestValidationCantRetrieveManifest(this ILogger logger, string videoId, SwarmReference manifestReference, Exception? exception) =>
            _videoManifestValidationCantRetrieveManifest(logger, videoId, manifestReference, exception!);

        public static void VideoManifestValidationFailedWithErrors(this ILogger logger, string videoId, SwarmReference manifestReference, Exception? exception) =>
            _videoManifestValidationFailedWithErrors(logger, videoId, manifestReference, exception!);

        public static void VideoManifestValidationRetrievedManifest(this ILogger logger, string videoId, SwarmReference manifestReference) =>
            _videoManifestValidationRetrievedManifest(logger, videoId, manifestReference, null!);

        public static void VideoManifestValidationStarted(this ILogger logger, string videoId, SwarmReference manifestReference) =>
            _videoManifestValidationStarted(logger, videoId, manifestReference, null!);

        public static void VideoManifestValidationSucceeded(this ILogger logger, string videoId, SwarmReference manifestReference) =>
            _videoManifestValidationSucceeded(logger, videoId, manifestReference, null!);

        public static void VideoVoted(this ILogger logger, string userId, string videoId) =>
            _videoVoted(logger, userId, videoId, null!);
    }
}
