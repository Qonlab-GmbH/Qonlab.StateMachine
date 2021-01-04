using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qonlab.StateMachine {
    public class StateMachineTransitionFilter {
        public bool IncludeManual { get; set; }
        public bool IncludeUnauthorized { get; set; }

    }
}
