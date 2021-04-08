using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace KT_Interface.Extensions
{
    public static class StoryboardExtensions
    {
        public static Task BeginAsync(this Storyboard storyboard)
        {
            var tcs = new TaskCompletionSource<bool>();
            if (storyboard == null)
            {
                tcs.SetException(new ArgumentNullException());
            }
            else
            {
                EventHandler onComplete = null;
                onComplete = (s, e) => {
                    storyboard.Completed -= onComplete;
                    tcs.SetResult(true);
                };

                storyboard.Completed += onComplete;
                storyboard.Begin();
            }

            return tcs.Task;
        }
    }
}
