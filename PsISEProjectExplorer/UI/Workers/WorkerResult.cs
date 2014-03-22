using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.UI.Workers
{
    public class WorkerResult
    {
        public DateTime StartTimestamp { get; private set; }

        public object Result { get; private set; }

        public WorkerResult(DateTime startTimeStamp, object result)
        {
            this.StartTimestamp = startTimeStamp;
            this.Result = result;
        }
    }
}
