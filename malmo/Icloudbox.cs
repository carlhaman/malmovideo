using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace malmo
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "Icloudbox" in both code and config file together.
    [ServiceContract]
    public interface Icloudbox
    {
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "get_smil?video_id={video_id}")]
        [OperationContract]
        System.IO.Stream get_smil(string video_id);
    }
}
