﻿namespace MyIntrinsic.View;

public partial class AboutView : ContentPage
{
    public AboutView(AboutViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}