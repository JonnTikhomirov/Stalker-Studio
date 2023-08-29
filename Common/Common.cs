using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stalker_Studio.Common
{
    /// <summary>
    /// Именнованный объект
    /// </summary>
    interface INammed
    {
        /// <summary>
        /// Имя
        /// </summary>
        [
            System.ComponentModel.DisplayName("Имя"),
            System.ComponentModel.Description("Имя объекта"),
            PropertyTools.DataAnnotations.SortIndex(-1)
        ]
        string Name
        {
            get;
            set;
        }
    }
}
