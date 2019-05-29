﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using tickets.API;
using Xamarin.Forms;
using System.Collections.ObjectModel;
using System.Timers;
using Acr.UserDialogs;
using SQLite;

namespace tickets
{
    public partial class MyTickets : ContentPage
    {
        private Server server = new Server();
        
        List<Ticket> tickets = new List<Ticket>();
        bool sentTicket = false;
        SendTicket view_sendTicket = new SendTicket();


        public MyTickets()
        {
            try
            {
                InitializeComponent();
                //UserDialogs.Instance.ShowLoading("Cargando Tickets...");
                GetTickets();
                TicketsListView.ItemsSource = tickets;
              
                this.BindingContext = this;
              
                //UserDialogs.Instance.HideLoading();
                var newTicket = new ToolbarItem
                {
                    Icon = "nuevo.jpg",
                    Command = new Command(async (x) => await Navigation.PushAsync(view_sendTicket)),
                 
                    Order = ToolbarItemOrder.Primary

                };
               

                var settings = new ToolbarItem
                {
                    
                    Text = "Ajustes",
                    Command = new Command(async (s) => await Navigation.PushAsync(new AppSettingsPage())),
                      
                    Order = ToolbarItemOrder.Secondary
                   
                };

                var addTicketTool = new ToolbarItem
                {
                
                    Text = "Agregar Ticket",
                    Command = new Command(execute: () => addTicketIdAsync()),

                    Order = ToolbarItemOrder.Secondary

                };
              

                switch (Device.RuntimePlatform)
                {
                    case Device.iOS:
                        ToolbarItems.Add(newTicket);
                        break;
                    case Device.Android:
                        ToolbarItems.Add(newTicket);
                        ToolbarItems.Add(settings);
                        ToolbarItems.Add(addTicketTool);
                        break;
                    case Device.UWP:
                        ToolbarItems.Add(newTicket);
                        ToolbarItems.Add(settings);
                        break;
                }
               
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }



        

        private async Task addTicketIdAsync()
        {
            Console.WriteLine("ADD TICKET FROM ID");
            var promptConfig = new PromptConfig
            {
                InputType = InputType.Name,
                IsCancellable = true,
                Message = "INGRESE ID DE TICKET"
            };
            var result = await UserDialogs.Instance.PromptAsync(promptConfig);
            if (result.Ok)
            {
                //string error = "No se agrego el ticket, su numero de cuenta no coincide con el numero de cuenta enlazado al ticket";
                if (result.Text == "")
                {
                    //error = "Ingrese un id";
                    UserDialogs.Instance.ShowError("Ingrese un id.");
                }
                else
                {
                    UserDialogs.Instance.ShowLoading("Por favor espere");
                    User current = await App.Database.GetCurrentUser();
                  //  UserDialogs.Instance.ShowError(current.Account);
                    string html = await server.getDetailsTicket(result.Text);
                    string date = await server.getInitDate(result.Text);
                    
                    UserDialogs.Instance.HideLoading();
                    if (html == "Error")
                    {
                        //error = "No existe un ticket con ese numero de ID: " + result.Text;
                        UserDialogs.Instance.ShowError("No existe un ticket con ese number de ID: "+result.Text);
                    }
                    else
                    {
                        //string account = getDetailTicket(html, "No. de talento Humano: ");
                       // Console.WriteLine("Server Account: " + account);
                        
                        if (getIDAccount(html,current.Account))
                        {
                            string c = getDetailTicket(html, "Clasificacion:");
                            int clas = 5;
                            if (c == "Solicitud"){
                                clas = 1;
                            }
                            else if (c == "Información"){
                                clas = 2;
                            }
                            else if (c == "Queja"){
                                clas = 3;
                            }
                            else if (c == "Reclamo"){
                                clas = 4;
                            }
                            string prioridad = getDetailTicket(html, "Prioridad:");
                            int p = 3;
                            if (prioridad == "Alto"){
                                p = 1;
                            }
                            else if (prioridad == "Medio")
                            {
                                p = 2;
                            }
                            try
                            {
                                await App.Database.CreateNewTicket(new Ticket()
                                {
                                    ID = result.Text,
                                    UserID = current.ID,
                                    Affected = int.Parse(getDetailTicket(html, "Cantidad de usuarios afectados:")),
                                    Classification = clas,
                                    Priority = p,
                                    Subject = getDetailTicket(html, "Tema"),
                                    Message = getDetailTicket(html, "<b>Mensaje:</b>"),
                                    Date = date,
                                });
                                //error = "El ticket se agrego exitosamente";
                                this.GetTickets();
                                UserDialogs.Instance.ShowSuccess("Ticket Agregado!");
                               
                            }
                            catch (SQLiteException)
                            {
                                //error = "No se agrergo el ticket, porque ya existe en la aplicacion";
                                UserDialogs.Instance.ShowError("No se agrego el ticket, porque ya existe en la base de datos.");
                            }
                        }
                        else
                        {
                            UserDialogs.Instance.ShowError("No se agrego el ticket, su numero de cuenta no coincide con el numero de cuenta enlazado al ticket");
                        }
                        

                    }
                }               
            }

        }

        //FUNCIONES AGREGAR TICKET DESDE ID
        private string getDetailTicket(string html, string search)
        {
            int pos = html.IndexOf(search) + search.Length;
            html = html.Substring(pos);
            pos = 0;
            string detail = "";
            if (search == "Tema")
            {
                search = "<b>";
                pos = html.IndexOf(search) + search.Length;
            }
            else if (search == "<b>Mensaje:</b>")
            {
                search = "<br />";
                pos = html.IndexOf(search) + search.Length;
            }
            else
            {
                pos = pos + 1;
            }
            detail = server.getTextAux('<', html, pos);
            Console.WriteLine("Detalle: " + detail);
            return detail;
        }



        private bool getIDAccount(string html,string numberAccount)
        {
            return html.Contains("No. de talento Humano: " + numberAccount+"<");
        }


        //TERMINAN FUNCIONES


        protected override async void OnAppearing()
        {
            base.OnAppearing();

            Device.StartTimer(new TimeSpan(0, 0, 1), () =>
            {
                if (view_sendTicket.sentTicket)
                {
                    Console.WriteLine("Refrescando todos los tickets");
                    this.GetTickets();
                    view_sendTicket = new SendTicket();
                }
                return true;
            });
           /*
            Device.StartTimer(new TimeSpan(0, 0, AppSettings.RefreshTicketsTimeout), () =>
              {
                  if (!view_sendTicket.sentTicket)
                  {
                      Console.WriteLine("\n\n" + "sent ticket:\t" + view_sendTicket.sentTicket + "\n\n");
                      GetTickets();
                      
                  }
                  return true;

              });*/
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();
        }

        async void goToViewTicket(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem != null)
            {
                var ticket = tickets.FirstOrDefault(t => t.ID == ((Ticket)e.SelectedItem).ID);
                if (ticket != null)
                {
                    UserDialogs.Instance.ShowLoading("Cargando Ticket...");
                    Debug.WriteLine("Opening messages for ticket with id = " + ticket.ID);
                    ticket.Date = await server.getUpdateDate(ticket.ID);
                    ticket.Image = "";

                    ticket.OpenImage = "";

                    await App.Database.UpdateTicket(ticket);
                    await Navigation.PushAsync(new chatTicket()
                    {
                        BindingContext = ticket.ID
                    });
                    TicketsListView.SelectedItem = null;
                    UserDialogs.Instance.HideLoading();
                }
                else
                {
                    Debug.WriteLine("Ticket is null");
                }
            }
        }

        private async void TicketsListView_Refreshing(object sender, EventArgs e)
        {

           
            this.GetTickets();
            TicketsListView.EndRefresh();
        }

        private async void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            //List<Ticket> tickets = await App.Database.GetTicketsAsync(App.Database.GetCurrentUserNotAsync());

            if (!String.IsNullOrWhiteSpace(e.NewTextValue))
            {
                var showTickets = tickets.Where(t => t.Subject.Contains(e.NewTextValue)).ToList();
                TicketsListView.ItemsSource = showTickets;
            }
            else
            {
                TicketsListView.ItemsSource = tickets;
            }
        }

        public async void GetTickets()
        {
            try {

                
                List<Ticket> dbtickets;
                tickets.Clear();
               // App.Database.ClearTicket();
                dbtickets = await App.Database.GetTicketsAsync();
                                                                           
                
                /*for (int i = 0; i < dbtickets.Count; i++){
                //    Console.WriteLine("DBTICKETS COUNT: " +dbtickets.Count);
                //    String updateDate = await server.getUpdateDate(dbtickets[i].ID);
                    Console.WriteLine("Recibiendo del sevidor para notificacion: " + updateDate.ToString());
                    
                    if (!updateDate.Equals(dbtickets[i].Date)/ && updateDate != "error"){
                        dbtickets[i].Image = "bell.png";
                        dbtickets[i].Date = updateDate;
                        await App.Database.UpdateTicket(dbtickets[i]);
                    }
                    else
                        dbtickets[i].Image = "";

                    bool open = await server.getOpenTicket(dbtickets[i].ID);
                    Console.WriteLine("Recibiendo del sevidor: "+ open.ToString());
                    if (!open){
                        dbtickets[i].OpenImage = "lock.png";
                        dbtickets[i].Open = open;
                        await App.Database.UpdateTicket(dbtickets[i]);
                    }
                    else
                        dbtickets[i].OpenImage = "";
              
                    var exists = tickets.FirstOrDefault(t => t.ID == dbtickets[i].ID);
                    // if no ticket was found with that id
                    if (exists == null) {
                        tickets.Add(dbtickets[i]);
                    }
                    else{
                        exists.Image = dbtickets[i].Image;
                        exists.OpenImage = dbtickets[i].OpenImage;
                        if (!updateDate.Equals(exists)){
                            exists.Date = updateDate;
                        }

                    }
                }
*/              
                TicketsListView.ItemsSource = null;
                TicketsListView.ItemsSource = tickets;               

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
