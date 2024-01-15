using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

// Define the Schedule interface
public interface IOverlappable
{
    bool Overlaps(Schedule other);
}

// Define the Schedule class
public class Schedule : IOverlappable
{
    public DateTime PickupDate { get; set; }
    public DateTime DropoffDate { get; set; }

    public bool Overlaps(Schedule other)
    {
        return PickupDate < other.DropoffDate && DropoffDate > other.PickupDate;
    }
}

// Define the Vehicle class
public class Vehicle
{
    public string RegistrationNumber { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public decimal DailyRentalPrice { get; set; }
    public string VehicleType { get; set; }

    public Vehicle(string registrationNumber, string make, string model, decimal dailyRentalPrice, string vehicleType)
    {
        RegistrationNumber = registrationNumber;
        Make = make;
        Model = model;
        DailyRentalPrice = dailyRentalPrice;
        VehicleType = vehicleType;
    }
}

// Define the WestminsterRentalVehicle class
public class WestminsterRentalVehicle : IRentalManager, IRentalCustomer
{
    private List<Vehicle> vehicles = new List<Vehicle>();
    private List<(Vehicle, Schedule, string)> reservations = new List<(Vehicle, Schedule, string)>();

    public bool AddVehicle(Vehicle v)
    {
        if (!vehicles.Any(vehicle => vehicle.RegistrationNumber == v.RegistrationNumber))
        {
            vehicles.Add(v);
            Console.WriteLine($"Vehicle {v.RegistrationNumber} added. Available parking lots: {50 - vehicles.Count}");
            return true;
        }
        else
        {
            Console.WriteLine("Duplicate registration number. Vehicle not added.");
            return false;
        }
    }

    public bool DeleteVehicle(string number)
    {
        Vehicle vehicleToDelete = vehicles.FirstOrDefault(vehicle => vehicle.RegistrationNumber == number);

        if (vehicleToDelete != null)
        {
            vehicles.Remove(vehicleToDelete);
            Console.WriteLine($"Vehicle {vehicleToDelete.RegistrationNumber} deleted. Available parking lots: {50 - vehicles.Count}");
            return true;
        }
        else
        {
            Console.WriteLine($"Vehicle with registration number {number} not found.");
            return false;
        }
    }

    public void ListVehicles()
    {
        foreach (var vehicle in vehicles)
        {
            Console.WriteLine($"Registration Number: {vehicle.RegistrationNumber}, Make: {vehicle.Make}, Model: {vehicle.Model}");
            // Additional information as needed
        }
    }

    public void ListOrderedVehicles()
    {
        var orderedVehicles = vehicles.OrderBy(vehicle => vehicle.Make);
        foreach (var vehicle in orderedVehicles)
        {
            Console.WriteLine($"Registration Number: {vehicle.RegistrationNumber}, Make: {vehicle.Make}, Model: {vehicle.Model}");
            // Additional information as needed
        }
    }

    public void GenerateReport(string fileName)
    {
        using (StreamWriter writer = new StreamWriter(fileName))
        {
            foreach (var vehicle in vehicles)
            {
                writer.WriteLine($"Vehicle Information - Registration Number: {vehicle.RegistrationNumber}, Make: {vehicle.Make}, Model: {vehicle.Model}");

                // Retrieve bookings for the current vehicle and sort them by start date
                var vehicleBookings = reservations.Where(r => r.Item1 == vehicle).OrderBy(r => r.Item2.PickupDate);

                foreach (var booking in vehicleBookings)
                {
                    writer.WriteLine($"  Booking Details - Pickup Date: {booking.Item2.PickupDate}, Dropoff Date: {booking.Item2.DropoffDate}");
                    writer.WriteLine($"    Driver Details - {booking.Item3}");
                }

                writer.WriteLine();
            }

            Console.WriteLine($"Report generated successfully. Saved to {fileName}");
        }
    }

    public void ListAvailableVehicles(Schedule wantedSchedule, string vehicleType)
    {
        var availableVehicles = vehicles
            .Where(vehicle => vehicle.VehicleType.Equals(vehicleType, StringComparison.OrdinalIgnoreCase) && IsVehicleAvailable(vehicle, wantedSchedule))
            .ToList();

        if (availableVehicles.Any())
        {
            Console.WriteLine($"Available {vehicleType}s for the requested schedule:");

            foreach (var vehicle in availableVehicles)
            {
                Console.WriteLine($"Registration Number: {vehicle.RegistrationNumber}, Make: {vehicle.Make}, Model: {vehicle.Model}");
                // Additional information as needed
            }
        }
        else
        {
            Console.WriteLine($"No available {vehicleType}s for the requested schedule.");
        }

    }
    public bool IsVehicleAvailable(Vehicle vehicle, Schedule wantedSchedule)
    {
        // Check if there are any reservations for the vehicle
        var vehicleReservations = reservations.Where(r => r.Item1 == vehicle).ToList();

        // Check if the wanted schedule overlaps with any existing reservations
        foreach (var reservation in vehicleReservations)
        {
            if (wantedSchedule.Overlaps(reservation.Item2))
            {
                return false; // Overlapping schedule, vehicle is not available
            }
        }

        return true; // No overlapping schedules, vehicle is available
    }
 


        /// <summary>
        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// </summary>


        public bool AddReservation(string number, Schedule wantedSchedule)
    {
        Vehicle selectedVehicle = vehicles.FirstOrDefault(vehicle => vehicle.RegistrationNumber == number);

        if (selectedVehicle == null)
        {
            Console.WriteLine($"Vehicle with registration number {number} not found. Reservation not added.");
            return false;
        }

        // Check for overlapping schedules
        if (reservations.Any(r => r.Item1 == selectedVehicle && r.Item2.Overlaps(wantedSchedule)))
        {
            Console.WriteLine($"Reservation overlaps with an existing booking. Reservation not added.");
            return false;
        }

        // Get driver details
        Console.Write("Enter driver's name: ");
        string name = Console.ReadLine();
        Console.Write("Enter driver's surname: ");
        string surname = Console.ReadLine();
        Console.Write("Enter driver's date of birth (MM/DD/YYYY): ");
        DateTime dob = DateTime.ParseExact(Console.ReadLine(), "MM/dd/yyyy", CultureInfo.InvariantCulture);
        Console.Write("Enter driver's license number: ");
        string licenseNumber = Console.ReadLine();

        string driverDetails = $"{name} {surname}, DOB: {dob.ToShortDateString()}, License: {licenseNumber}";

        // Calculate total price based on daily rental price
        decimal totalPrice = (decimal)(wantedSchedule.DropoffDate - wantedSchedule.PickupDate).TotalDays * selectedVehicle.DailyRentalPrice;

        // Add the reservation to the list
        reservations.Add((selectedVehicle, wantedSchedule, driverDetails));

        Console.WriteLine($"Reservation added successfully. Total price: {totalPrice:C}");
        return true;
    }

    /// <summary>
    /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>
    
    public bool ChangeReservation(string number, Schedule oldSchedule, Schedule newSchedule)
    {
        Vehicle selectedVehicle = vehicles.FirstOrDefault(vehicle => vehicle.RegistrationNumber == number);

        if (selectedVehicle == null)
        {
            Console.WriteLine($"Vehicle with registration number {number} not found. Reservation not changed.");
            return false;
        }

        // Check if the new schedule overlaps with existing reservations
        if (reservations.Any(r => r.Item1 == selectedVehicle && r.Item2 != oldSchedule && r.Item2.Overlaps(newSchedule)))
        {
            Console.WriteLine($"New schedule overlaps with an existing booking. Reservation not changed.");
            return false;
        }

        // Find the existing reservation
        var existingReservation = reservations.FirstOrDefault(r => r.Item1 == selectedVehicle && r.Item2 == oldSchedule);

        if (existingReservation == default)
        {
            Console.WriteLine($"Reservation not found for the specified schedule. Reservation not changed.");
            return false;
        }

        // Update the reservation with the new schedule
        reservations.Remove(existingReservation);
        reservations.Add((selectedVehicle, newSchedule, existingReservation.Item3));

        Console.WriteLine("Reservation changed successfully.");
        return true;
    }

    

    /// <summary>
    /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>
    
   
    public bool DeleteReservation(string number, Schedule schedule)
    {
    Vehicle selectedVehicle = vehicles.FirstOrDefault(vehicle => vehicle.RegistrationNumber == number);

    if (selectedVehicle == null)
    {
        Console.WriteLine($"Vehicle with registration number {number} not found. Reservation not deleted.");
        return false;
    }

    // Find the existing reservation
    var existingReservation = reservations.FirstOrDefault(r => r.Item1 == selectedVehicle && r.Item2 == schedule);

    if (existingReservation == default)
    {
        Console.WriteLine($"Reservation not found for the specified schedule. Reservation not deleted.");
        return false;
    }

    // Remove the reservation from the list
    reservations.Remove(existingReservation);

    Console.WriteLine("Reservation deleted successfully.");
    return true;

    }
    
    // Additional methods and properties as needed
}

// Define the IRentalManager and IRentalCustomer interfaces
public interface IRentalManager
{
    bool AddVehicle(Vehicle v);
    bool DeleteVehicle(string number);
    void ListVehicles();
    void ListOrderedVehicles();
    void GenerateReport(string fileName);
}

public interface IRentalCustomer
{
    void ListAvailableVehicles(Schedule wantedSchedule, string vehicleType);
    bool AddReservation(string number, Schedule wantedSchedule);
    bool ChangeReservation(string number, Schedule oldSchedule, Schedule newSchedule);
    bool DeleteReservation(string number, Schedule schedule);
}

// Define the main program
class Program
{
    static void Main()
    {
        WestminsterRentalVehicle rentalSystem = new WestminsterRentalVehicle();

        while (true)
        {
            Console.WriteLine("1. Customer Menu");
            Console.WriteLine("2. Admin Menu");
            Console.WriteLine("3. Exit");

            int choice;
            if (int.TryParse(Console.ReadLine(), out choice))
            {
                switch (choice)
                {
                    case 1:
                        CustomerMenu(rentalSystem);
                        break;
                    case 2:
                        AdminMenu(rentalSystem);
                        break;
                    case 3:
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a valid number.");
            }
        }
    }

    static void CustomerMenu(WestminsterRentalVehicle rentalSystem)
    {
        while (true)
        {
            Console.WriteLine("Customer Menu");
            Console.WriteLine("1. List Available Vehicles");
            Console.WriteLine("2. Add Reservation");
            Console.WriteLine("3. Change Reservation");
            Console.WriteLine("4. Delete Reservation");
            Console.WriteLine("5. Back to Main Menu");

            int choice;
            if (int.TryParse(Console.ReadLine(), out choice))
            {
                switch (choice)
                {
                    case 1:
                        Console.Write("Enter the schedule details (Pickup Date(MM/DD/YYYY) - Dropoff Date(MM/DD/YYYY)): ");
                        string[] scheduleDetails = Console.ReadLine().Split('-');
                        DateTime pickupDate, dropoffDate;
                        if (DateTime.TryParseExact(scheduleDetails[0].Trim(), "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out pickupDate)
                            && DateTime.TryParseExact(scheduleDetails[1].Trim(), "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dropoffDate))
                        {
                            Schedule iwantedSchedule = new Schedule { PickupDate = pickupDate, DropoffDate = dropoffDate };

                            Console.WriteLine("Enter the type of vehicle (Car, Van, ElectricCar, Motorbike): ");
                            string vehicleType = Console.ReadLine();

                            rentalSystem.ListAvailableVehicles(iwantedSchedule, vehicleType);
                        }
                        else
                        {
                            Console.WriteLine("Invalid schedule format. Please enter dates in MM/DD/YYYY format.");
                        }
                        break;

                    case 2:
                        Console.Write("Enter the registration number of the vehicle: ");
                        string vehicleNumber = Console.ReadLine();

                        Console.Write("Enter the pickup date (MM/DD/YYYY): ");
                        DateTime ipickupDate;
                        if (!DateTime.TryParseExact(Console.ReadLine(), "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ipickupDate))
                        {
                            Console.WriteLine("Invalid date format. Please enter the date in MM/DD/YYYY format.");
                            // Handle invalid input (e.g., return, ask user to enter the information again)
                            break;
                        }

                        Console.Write("Enter the dropoff date (MM/DD/YYYY): ");
                        DateTime idropoffDate;
                        if (!DateTime.TryParseExact(Console.ReadLine(), "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out idropoffDate))
                        {
                            Console.WriteLine("Invalid date format. Please enter the date in MM/DD/YYYY format.");
                            // Handle invalid input (e.g., return, ask user to enter the information again)
                            break;
                        }

                        Schedule wantedSchedule = new Schedule { PickupDate = ipickupDate, DropoffDate = idropoffDate };

                        rentalSystem.AddReservation(vehicleNumber, wantedSchedule);
                        break;

                    case 3:
                        Console.Write("Enter the registration number of the vehicle: ");
                        string changeVehicleNumber = Console.ReadLine();

                        Console.Write("Enter the current pickup date (MM/DD/YYYY): ");
                        DateTime currentPickupDate;
                        if (!DateTime.TryParseExact(Console.ReadLine(), "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out currentPickupDate))
                        {
                            Console.WriteLine("Invalid date format. Please enter the date in MM/DD/YYYY format.");
                            // Handle invalid input (e.g., return, ask the user to enter the information again)
                            break;
                        }

                        Console.Write("Enter the current dropoff date (MM/DD/YYYY): ");
                        DateTime currentDropoffDate;
                        if (!DateTime.TryParseExact(Console.ReadLine(), "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out currentDropoffDate))
                        {
                            Console.WriteLine("Invalid date format. Please enter the date in MM/DD/YYYY format.");
                            // Handle invalid input (e.g., return, ask the user to enter the information again)
                            break;
                        }

                        Schedule oldSchedule = new Schedule { PickupDate = currentPickupDate, DropoffDate = currentDropoffDate };

                        Console.Write("Enter the new pickup date (MM/DD/YYYY): ");
                        DateTime newPickupDate;
                        if (!DateTime.TryParseExact(Console.ReadLine(), "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out newPickupDate))
                        {
                            Console.WriteLine("Invalid date format. Please enter the date in MM/DD/YYYY format.");
                            // Handle invalid input (e.g., return, ask the user to enter the information again)
                            break;
                        }

                        Console.Write("Enter the new dropoff date (MM/DD/YYYY): ");
                        DateTime newDropoffDate;
                        if (!DateTime.TryParseExact(Console.ReadLine(), "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out newDropoffDate))
                        {
                            Console.WriteLine("Invalid date format. Please enter the date in MM/DD/YYYY format.");
                            // Handle invalid input (e.g., return, ask the user to enter the information again)
                            break;
                        }

                        Schedule newSchedule = new Schedule { PickupDate = newPickupDate, DropoffDate = newDropoffDate };

                        rentalSystem.ChangeReservation(changeVehicleNumber, oldSchedule, newSchedule);
                        break;

                    case 4:
                        Console.Write("Enter the registration number of the vehicle: ");
                        string deleteVehicleNumber = Console.ReadLine();

                        Console.Write("Enter the pickup date (MM/DD/YYYY): ");
                        DateTime deletePickupDate;
                        if (!DateTime.TryParseExact(Console.ReadLine(), "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out deletePickupDate))
                        {
                            Console.WriteLine("Invalid date format. Please enter the date in MM/DD/YYYY format.");
                            // Handle invalid input (e.g., return, ask user to enter the information again)
                            break;
                        }

                        Console.Write("Enter the dropoff date (MM/DD/YYYY): ");
                        DateTime deleteDropoffDate;
                        if (!DateTime.TryParseExact(Console.ReadLine(), "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out deleteDropoffDate))
                        {
                            Console.WriteLine("Invalid date format. Please enter the date in MM/DD/YYYY format.");
                            // Handle invalid input (e.g., return, ask user to enter the information again)
                            break;
                        }

                        Schedule deleteSchedule = new Schedule { PickupDate = deletePickupDate, DropoffDate = deleteDropoffDate };

                        rentalSystem.DeleteReservation(deleteVehicleNumber, deleteSchedule);
                        break;

                    case 5:
                        return;

                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a valid number.");
            }
        }
    }

    static void AdminMenu(WestminsterRentalVehicle rentalSystem)
    {
        while (true)
        {
            Console.WriteLine("Admin Menu");
            Console.WriteLine("1. Add Vehicle");
            Console.WriteLine("2. Delete Vehicle");
            Console.WriteLine("3. List Vehicles");
            Console.WriteLine("4. List Ordered Vehicles");
            Console.WriteLine("5. Generate Report");
            Console.WriteLine("6. Back to Main Menu");

            int choice;
            if (int.TryParse(Console.ReadLine(), out choice))
            {
                switch (choice)
                {
                    case 1:
                        Console.WriteLine("Enter vehicle details:");
                        Console.Write("Registration Number: ");
                        string regNumber = Console.ReadLine();
                        Console.Write("Make: ");
                        string make = Console.ReadLine();
                        Console.Write("Model: ");
                        string model = Console.ReadLine();
                        Console.Write("Daily Rental Price: ");
                        decimal dailyRentalPrice;
                        if (decimal.TryParse(Console.ReadLine(), out dailyRentalPrice))
                        {
                            Console.Write("Vehicle Type (Car, Van, ElectricCar, Motorbike): ");
                            string vehicleType = Console.ReadLine();

                            Vehicle newVehicle = new Vehicle(regNumber, make, model, dailyRentalPrice, vehicleType)
                            {
                                RegistrationNumber = regNumber,
                                Make = make,
                                Model = model,
                                DailyRentalPrice = dailyRentalPrice,
                                VehicleType = vehicleType
                                // Additional properties as needed
                            };

                            rentalSystem.AddVehicle(newVehicle);
                        }
                        else
                        {
                            Console.WriteLine("Invalid daily rental price. Please enter a valid decimal value.");
                        }
                        break;


                    case 2:
                        Console.Write("Enter the registration number of the vehicle to delete: ");
                        string regNumberToDelete = Console.ReadLine();
                        rentalSystem.DeleteVehicle(regNumberToDelete);
                        break;

                    case 3:
                        rentalSystem.ListVehicles();
                        break;

                    case 4:
                        rentalSystem.ListOrderedVehicles();
                        break;

                    case 5:
                        Console.Write("Enter the file name for the report: ");
                        string fileName = Console.ReadLine();
                        rentalSystem.GenerateReport(fileName);
                        break;

                    case 6:
                        return;

                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a valid number.");
            }
        }
    }
}
