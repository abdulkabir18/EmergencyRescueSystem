using Domain.Enums;

namespace Application.Common.Helpers
{
    public static class MediaTypeMapper
    {
        public static MediaType MapContentType(string contentType) =>
            contentType switch
            {
                var s when s.StartsWith("image/") => MediaType.Image,
                var s when s.StartsWith("video/") => MediaType.Video,
                var s when s.StartsWith("audio/") => MediaType.Audio,
                _ => throw new ArgumentException($"Unsupported content type: {contentType}")
            };
    }
}