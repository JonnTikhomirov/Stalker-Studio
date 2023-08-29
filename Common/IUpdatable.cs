using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stalker_Studio.Common
{
    /// <summary>
    /// Процедура обновления информации о разметке элементов текста (смещения и т.д.)
    /// </summary>
    public interface IUpdatable
    {
        void Update(IUpdatable source);
    }
}
