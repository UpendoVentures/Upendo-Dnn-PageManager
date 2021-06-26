using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Upendo.Modules.DnnPageManager.Model
{
	public class Outcome
	{
		public bool Success { get; set; }
		public string ErrorMessage { get; set; }
		public string Suggestion { get; set; }
	}
}
