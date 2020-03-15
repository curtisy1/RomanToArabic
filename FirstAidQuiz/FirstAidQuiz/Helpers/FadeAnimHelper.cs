using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FirstAidQuiz.Helpers {

    internal static class FadeAnimHelper {
        private const int FADESPEED = 1;
        private const int DIVIDER = 15;

        public static async void FadeLabel(object sender, Color color, CancellationToken cancellationToken, int endHeight = -1, int endWidth = -1) {
            var label = (Label)sender;
            if (endHeight == -1) {
                endHeight = label.Height;
                //TODO resizing missing
            }
            if (endWidth == -1) {
                endWidth = label.Width;
            }
            var col = label.ForeColor;
            var oldCol = new List<int> { col.R, col.G, col.B };
            var finalCol = new List<int> { color.R, color.G, color.B };
            var colDif = new List<int> { finalCol[0] - oldCol[0], finalCol[1] - oldCol[1], finalCol[2] - oldCol[2] };
            var heightDif = endHeight - label.Height;

            for (var i = 0; i <= DIVIDER; i++) {
                var c = Color.FromArgb(255, oldCol[0] + (colDif[0] / DIVIDER) * i, oldCol[1] + (colDif[1] / DIVIDER) * i, oldCol[2] + (colDif[2] / DIVIDER) * i);
                label.ForeColor = c;
                label.Height = label.Height + (heightDif / 100) * i;

                if (cancellationToken.IsCancellationRequested)
                    return;

                await Task.Delay(FADESPEED).ConfigureAwait(false);
            }
        }
    }
}