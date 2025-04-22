using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EyeInTheSky.ClassLibrary
{
    public class MarsPhoto
    {       
        public string Img_src { get; set; }
    }
    public class MarsPhotoResponse
    {
        public List<MarsPhoto> Photos { get; set; }
    }
}
