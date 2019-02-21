﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tickets.API;
using tickets.Models;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Http.Headers;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Xamarin.Forms;
using Xamarin.Essentials;

using Xamarin.Forms.Xaml;
using Xamarin.Forms;


namespace tickets.screens
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPageAdmin : ContentPage
    {
        private Server server = new Server();
        private AdminLogin admin_log = new AdminLogin();
        public LoginPageAdmin()
        {
            InitializeComponent();
        }
        public async void SendRequest(object sender, System.EventArgs e)
        {
            admin_log.username = usuario.Text;
            admin_log.password = contrasena.Text;
            var valid = !String.IsNullOrWhiteSpace(usuario.Text) && !String.IsNullOrWhiteSpace(contrasena.Text);
            if (valid)
            {

                if (CheckInternetConnection())
                {
                    try
                    {
                        string response = await admin_log.loginAdmins();

                        if (response == " error")
                        {
                            await DisplayAlert("No se ha podido Acceder como Admin", "Revise por favor", "OK");
                        }
                        else if (response == "sucess")
                        {
                            label_contrasena.IsVisible = false;
                            usuario.IsVisible = false;
                            contrasena.IsVisible = false;
                            label_usuario.IsVisible = false;
                            login.IsVisible = false;
                            Loading.IsVisible = true;

                            switch (Xamarin.Forms.Device.RuntimePlatform)
                            {
                                case Xamarin.Forms.Device.iOS:
                                    App.Current.MainPage = new NavigationPage(new HomeScreen());
                                    break;
                                case Xamarin.Forms.Device.Android:
                                    App.Current.MainPage = new NavigationPage(new MyTicketsAdmin());
                                    break;

                            }

                        }
                    }
                    catch
                    {
                        await DisplayAlert("Error", "Revise el Admin", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Error", "Ingrese Datos", "OK");
                }

            }


            else
            {
                await DisplayAlert("No hay conexión", "No se detecto una conexión a Internet. Por favor vuelta a intentarlo", "Ok");

            }
        }
        public bool CheckInternetConnection()
        {
            var current = Connectivity.NetworkAccess;
            if (current == NetworkAccess.Internet)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

    }
}

