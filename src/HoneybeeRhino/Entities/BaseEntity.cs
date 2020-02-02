using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Rhino.Collections;
using Rhino.DocObjects.Custom;

namespace HoneybeeRhino.Entities
{
    [Guid("D9C8832F-EE24-4834-A443-AC981B8D9921")]
    public abstract class BaseEntity: UserData
    {

        private protected abstract ArchivableDictionary Serialize();
        private protected abstract void Deserialize(ArchivableDictionary dictionary);

    }

}