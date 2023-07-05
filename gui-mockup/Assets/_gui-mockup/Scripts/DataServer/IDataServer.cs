using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace _gui_mockup.DataServer
{
	public interface IDataServer
	{
		Task<int> DataAvailable(CancellationToken ct);
		Task<IList<DataItem>> RequestData(int index, int count, CancellationToken ct);
	}
}
