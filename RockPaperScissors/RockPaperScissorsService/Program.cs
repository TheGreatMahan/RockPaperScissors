/*
 *  Program:        RockPaperScissorsService.exe 
 *  Module:         Program.cs
 *  Author:         Ali, Dylan, Mahan, Manh
 *  Date:           March 18, 2022
 *  Description:    A host application for the RockPaperScissors WCF service
 */

using System;

using System.ServiceModel;
using RockPaperScissorsLibrary;

namespace RockPaperScissorsService
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceHost servHost = null;
            try
            {
                // Create the service host 
                servHost = new ServiceHost(typeof(RockPaperScissors));

                // Start the service
                servHost.Open();
                Console.WriteLine("Service started. Press a key to quit.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.ReadKey();
                if (servHost != null)
                    servHost.Close();
            }
        }
    }
}
