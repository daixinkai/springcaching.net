using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Serialization
{
    public interface ICacheSerializer
    {
        byte[] SerializeObject(object? value);
        TResult? DeserializeObject<TResult>(byte[] value);
    }
}
