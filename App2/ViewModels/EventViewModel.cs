using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TSSDataLogger.Data;
using Windows.Globalization.DateTimeFormatting;

namespace TSSDataLogger.ViewModels
{
    public class EventViewModel
    {
        private int _listId;

        public int listId
        {
            get
            {
                return _listId;
            }
        }

        public string DateCreatedHourMinute
        {
            get
            {
                var formatter = new DateTimeFormatter("hour minute");
                return formatter.Format(start);
            }
        }

        public string code { get; set; }
        public string description { get; set; }
        public DateTime start { get; set; }

        public EventViewModel()
        {
        }

        public static EventViewModel FromEvent(Data.Event item)
        {
            var viewModel = new EventViewModel();

            viewModel._listId = item.listId;
            viewModel.start = item.start;
            viewModel.code = item.code;
            viewModel.description = item.description;

            return viewModel;
        }
    }
}
