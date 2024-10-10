# SoftBusinessCards

SoftBusinessCards is a simple project that allows users to create business cards in different ways—either manually or by uploading files in various formats. The application also allows users to view, delete, or export their existing business cards.

## Technologies Used

- **Frontend:**  
  - Developed with **Angular 18**.
  - IDE: **Visual Studio Code**.

- **Backend:**  
  - Built using **.NET Core 6.0**.
  - IDE: **Visual Studio 2022**.
  - Database: **PostgreSQL**.
  - Libraries: 
    - **ZXing.net** (for QR code processing).
    - **CsvHelper** (for handling CSV files).
  - CORS is enabled to connect the frontend with the backend.

## Features

- Create business cards manually or by uploading files in multiple formats (CSV, QR code, etc.).
- View, delete, and export business cards.
- API endpoints are available to interact with the backend services.
- Unit tests written for service and controller layers using **NUnit** and **Moq**.
- API testing and endpoint validation performed using **Swagger**.

## Running the Project

To run this project locally, ensure you have the following prerequisites installed:

### Prerequisites

- **.NET 6.0 SDK**: Make sure you have .NET 6.0 installed on your machine. You can download it from [here](https://dotnet.microsoft.com/download/dotnet/6.0).
- **Node.js and npm**: Install the latest version of Node.js and npm for running Angular. You can download them from [here](https://nodejs.org/).

### Backend (.NET Core 6.0)

1. **Clone the repository**:
   git clone https://github.com/zaidmaani/SoftBusinessCards.git
   
2. Navigate to the backend project directory:
cd SoftBusinessCards/Backend

3. Restore dependencies:
dotnet restore

4. Set up the database:
Update the PostgreSQL connection string in appsettings.json.
Run the migrations to set up the database schema:
dotnet ef database update

5. Run the backend server:
dotnet run
The backend should now be running on http://localhost:7052.

### Frontend (Angular 18)

1. Navigate to the frontend project directory:
cd SoftBusinessCards/Frontend

2.Install frontend dependencies:
npm install

3.Run the frontend server:
ng serve
The frontend should now be running on http://localhost:4200.

Testing
Unit Tests:

Run the unit tests with NUnit:
dotnet test
API Testing:

Access the Swagger UI to test API endpoints: Navigate to http://localhost:7052/swagger in your browser.

## Conclusion

SoftBusinessCards is a flexible tool for managing business cards, supporting manual creation, file uploads, and easy management of existing cards. The backend is built using .NET Core 6.0, and the frontend uses Angular 18, making it scalable and maintainable. Feel free to explore and extend the functionality as needed!
