using System.Collections.Generic;

namespace AvatarSearcher
{
    internal class AvatarList
    {
        public List<Avatar> records { get; set; }
    }

    public class Avatar
    {
        public string avatarId { get; set; }
        public string avatarName { get; set; }
        public string avatarDescription { get; set; }
        public string authorId { get; set; }
        public string authorName { get; set; }
        public string imageUrl { get; set; }
        public string thumbnailUrl { get; set; }
        public string releaseStatus { get; set; }
        public string unityVersion { get; set; }
        public List<string> tags { get; set; }
        public string recordCreated { get; set; }
    }

    public class Root
    {
        public Avatar avatar { get; set; }
        public List<object> tags { get; set; }
    }
}
