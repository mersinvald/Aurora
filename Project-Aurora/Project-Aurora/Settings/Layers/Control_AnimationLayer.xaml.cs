﻿using Aurora.Controls;
using Aurora.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Aurora.Settings.Layers
{
    /// <summary>
    /// Interaction logic for Control_AnimationLayer.xaml
    /// </summary>
    public partial class Control_AnimationLayer : UserControl
    {
        private Window windowAnimationEditor = null;
        private static bool windowAnimationEditorOpen;

        private bool settingsset = false;
        private bool profileset = false;

        public Control_AnimationLayer()
        {
            InitializeComponent();

            // Populate trigger mode combobox
            foreach (var mode in Enum.GetValues(typeof(AnimationTriggerMode)).Cast<AnimationTriggerMode>())
                triggerModeCb.Items.Add(new KeyValuePair<string, AnimationTriggerMode>(mode.GetDescription(), mode));
            triggerModeCb.DisplayMemberPath = "Key";

            // Populate stack mode combobox
            foreach (var mode in Enum.GetValues(typeof(AnimationStackMode)).Cast<AnimationStackMode>())
                stackModeCb.Items.Add(new KeyValuePair<string, AnimationStackMode>(mode.GetDescription(), mode));
            stackModeCb.DisplayMemberPath = "Key";
        }

        public Control_AnimationLayer(AnimationLayerHandler datacontext) : this()
        {
            this.DataContext = datacontext;
        }

        private bool CanSet => IsLoaded && settingsset && DataContext is AnimationLayerHandler;
        private AnimationLayerHandler Context => DataContext as AnimationLayerHandler;

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetSettings();
            Loaded -= UserControl_Loaded;
        }
        
        public void SetSettings()
        {
            if(DataContext is AnimationLayerHandler && !settingsset)
            {
                chkboxForceKeySequence.IsChecked = Context.Properties._forceKeySequence;
                chkboxScaleToKeySequence.IsChecked = Context.Properties._scaleToKeySequenceBounds;
                KeySequence_keys.Sequence = Context.Properties._Sequence;
                updownAnimationDuration.Value = (double)Context.Properties._AnimationDuration;
                updownAnimationRepeat.Value = Context.Properties._AnimationRepeat;
                triggerModeCb.SelectedIndex = triggerModeCb.Items.SourceCollection.Cast<KeyValuePair<string, AnimationTriggerMode>>().Select((kvp, index) => new { kvp, index }).First(item => item.kvp.Value == Context.Properties.TriggerMode).index;
                triggerAnyKey.IsChecked = Context.Properties._TriggerAnyKey;
                triggerPath.Text = Context.Properties._TriggerPath;
                triggerKeys.Keybinds = Context.Properties._TriggerKeys;
                translateToKey.IsChecked = Context.Properties._KeyTriggerTranslate;
                stackModeCb.SelectedIndex = stackModeCb.Items.SourceCollection.Cast<KeyValuePair<string, AnimationStackMode>>().Select((kvp, index) => new { kvp, index }).First(item => item.kvp.Value == Context.Properties.StackMode).index;
                settingsset = true;
            }
        }

        internal void SetProfile(Profiles.Application profile)
        {
            if (profile != null && !profileset) {
                var var_types_numerical = profile.ParameterLookup?.Where(kvp => Utils.TypeUtils.IsNumericType(kvp.Value.Item1));
                triggerPath.Items.Clear();
                foreach (var item in var_types_numerical)
                    triggerPath.Items.Add(item.Key);
                profileset = true;
            }
            settingsset = false;
            SetSettings();
        }

        private void btnEditAnimation_Click(object sender, RoutedEventArgs e)
        {
            if (windowAnimationEditor == null)
            {
                if (windowAnimationEditorOpen == true)
                {
                    MessageBox.Show("Animation Editor already open for another layer.\r\nPlease close it.");
                    return;
                }

                windowAnimationEditor = new Window();
                windowAnimationEditor.Closed += WindowAnimationEditor_Closed;

                windowAnimationEditor.Title = "Animation Editor";

                Controls.Control_AnimationEditor animEditor = new Controls.Control_AnimationEditor() { AnimationMix = Context.Properties._AnimationMix };
                animEditor.AnimationMixUpdated += AnimEditor_AnimationMixUpdated;

                windowAnimationEditor.Content = animEditor;
                windowAnimationEditor.Background = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                windowAnimationEditor.Show();
            }
            else
            {
                windowAnimationEditor.BringIntoView();
            }

            windowAnimationEditorOpen = true;
        }

        private void WindowAnimationEditor_Closed(object sender, EventArgs e)
        {
            windowAnimationEditor = null;
            windowAnimationEditorOpen = false;
        }

        private void AnimEditor_AnimationMixUpdated(object sender, EffectsEngine.Animations.AnimationMix mix)
        {
            if (CanSet && sender is Controls.Control_AnimationEditor)
                (this.DataContext as AnimationLayerHandler).Properties._AnimationMix = mix;
        }

        private void chkboxForceKeySequence_Checked(object sender, RoutedEventArgs e)
        {
            if (CanSet && sender is CheckBox)
                Context.Properties._forceKeySequence = ((sender as CheckBox).IsChecked.HasValue ? (sender as CheckBox).IsChecked.Value : false);
        }

        private void chkboxScaleToKeySequence_Checked(object sender, RoutedEventArgs e)
        {
            if (CanSet && sender is CheckBox)
                Context.Properties._scaleToKeySequenceBounds = ((sender as CheckBox).IsChecked.HasValue ? (sender as CheckBox).IsChecked.Value : false);
        }

        private void KeySequence_keys_SequenceUpdated(object sender, EventArgs e)
        {
            if (CanSet && sender is Controls.KeySequence)
                Context.Properties._Sequence = (sender as Aurora.Controls.KeySequence).Sequence;
        }

        private void updownAnimationDuration_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (CanSet && sender is Xceed.Wpf.Toolkit.DoubleUpDown)
                Context.Properties._AnimationDuration = (float)((sender as Xceed.Wpf.Toolkit.DoubleUpDown).Value.HasValue ? (sender as Xceed.Wpf.Toolkit.DoubleUpDown).Value.Value : 0.0f);
        }

        private void updownAnimationRepeat_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (CanSet && sender is Xceed.Wpf.Toolkit.IntegerUpDown)
                Context.Properties._AnimationRepeat = ((sender as Xceed.Wpf.Toolkit.IntegerUpDown).Value.HasValue ? (sender as Xceed.Wpf.Toolkit.IntegerUpDown).Value.Value : 0);
        }

        private void triggerMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AnimationTriggerMode selectedItem = ((KeyValuePair<string, AnimationTriggerMode>)(sender as ComboBox).SelectedItem).Value;
            if (CanSet)
                Context.Properties._TriggerMode = selectedItem;

            // Only show trigger path when one of the path-like modes is set
            triggerGridLayout.RowDefinitions[1].Height = new GridLength(new[] { AnimationTriggerMode.OnHigh, AnimationTriggerMode.OnLow, AnimationTriggerMode.OnChange }.Contains(selectedItem) ? 28 : 0);
            // Only show tigger keys when one of the key-like modes is set
            triggerGridLayout.RowDefinitions[2].Height = new GridLength(selectedItem == AnimationTriggerMode.OnKeyPress || selectedItem == AnimationTriggerMode.OnKeyRelease ? 128 : 0);
            triggerGridLayout.RowDefinitions[3].Height = new GridLength(selectedItem == AnimationTriggerMode.OnKeyPress || selectedItem == AnimationTriggerMode.OnKeyRelease ? 28 : 0);
            // Only show the stack mode setting if the trigger mode is NOT "AlwaysOn"
            triggerGridLayout.RowDefinitions[4].Height = new GridLength(selectedItem == AnimationTriggerMode.AlwaysOn ? 0 : 28);
        }

        private void triggerPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CanSet)
                Context.Properties._TriggerPath = (sender as ComboBox).Text;
        }

        private void triggerAnyKey_Checked(object sender, RoutedEventArgs e) {
            bool val = (sender as CheckBox).IsChecked ?? false;
            if (CanSet)
                Context.Properties._TriggerAnyKey = val;

            // Disable keybind box if allow on any keys
            triggerKeys.IsEnabled = !val;
        }

        private void triggerKeys_KeybindsChanged(object sender) {
            if (CanSet)
                Context.Properties._TriggerKeys = (sender as KeyBindList).Keybinds;
        }

        private void translateToKey_Checked(object sender, RoutedEventArgs e) {
            if (CanSet)
                Context.Properties._KeyTriggerTranslate = (sender as CheckBox).IsChecked;
        }

        private void stackModeCb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CanSet)
                Context.Properties._StackMode = ((KeyValuePair<string, AnimationStackMode>)(sender as ComboBox).SelectedItem).Value;
        }

        private void btnInfo_Click(object sender, RoutedEventArgs e) {
            // Toggle the info text textblock and set the triggerGrid visibility to be the opposite
            triggerGridLayout.Visibility = infoText.Visibility;
            infoText.Visibility = infoText.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }        
    }
}
