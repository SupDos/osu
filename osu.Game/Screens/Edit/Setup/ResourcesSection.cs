// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;

namespace osu.Game.Screens.Edit.Setup
{
    internal class ResourcesSection : SetupSection
    {
        private LabelledTextBox audioTrackTextBox;

        public override LocalisableString Title => "Resources";

        [Resolved]
        private MusicController music { get; set; }

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [Resolved]
        private IBindable<WorkingBeatmap> working { get; set; }

        [Resolved(canBeNull: true)]
        private Editor editor { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            Container audioTrackFileChooserContainer = new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
            };

            Children = new Drawable[]
            {
                audioTrackTextBox = new FileChooserLabelledTextBox(".mp3", ".ogg")
                {
                    Label = "Audio Track",
                    PlaceholderText = "Click to select a track",
                    Current = { Value = working.Value.Metadata.AudioFile },
                    Target = audioTrackFileChooserContainer,
                    TabbableContentContainer = this
                },
                audioTrackFileChooserContainer,
            };

            audioTrackTextBox.Current.BindValueChanged(audioTrackChanged);
        }

        public bool ChangeAudioTrack(string path)
        {
            var info = new FileInfo(path);

            if (!info.Exists)
                return false;

            var set = working.Value.BeatmapSetInfo;

            // remove the previous audio track for now.
            // in the future we probably want to check if this is being used elsewhere (other difficulties?)
            var oldFile = set.Files.FirstOrDefault(f => f.Filename == working.Value.Metadata.AudioFile);

            using (var stream = info.OpenRead())
            {
                if (oldFile != null)
                    beatmaps.ReplaceFile(set, oldFile, stream, info.Name);
                else
                    beatmaps.AddFile(set, stream, info.Name);
            }

            working.Value.Metadata.AudioFile = info.Name;

            music.ReloadCurrentTrack();

            editor?.UpdateClockSource();
            return true;
        }

        private void audioTrackChanged(ValueChangedEvent<string> filePath)
        {
            if (!ChangeAudioTrack(filePath.NewValue))
                audioTrackTextBox.Current.Value = filePath.OldValue;
        }
    }
}
