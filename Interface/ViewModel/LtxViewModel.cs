using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stalker_Studio.StalkerClass;

namespace Stalker_Studio.ViewModel
{

    class LtxViewModel: FileViewModel
    {
        LtxViewModel(LtxModel file) : base(file)
        {
            Initialize();
        }
        LtxViewModel() : this(new LtxModel()) { }
        LtxViewModel(string path) : this(new LtxModel(path)) { }

        private void Initialize()
        {

        }
    }
}
