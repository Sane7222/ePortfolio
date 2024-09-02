# ePortfolio

This project is an ePortfolio website built with ASP.NET Core Razor Pages to consume and cache both Graph-QL and REST APIs. It leverages Azure for hosting and configuration management.

## Key Features
- **Contact Form**: Email the portfolio owner directly via FormSubmit.
- **API Integration**: Seamlessly consume and cache API responses.
- **CI/CD Pipeline**: Automated deployment to Azure App Service and API Management.
- **Configuration & Secrets Management**: Utilizes Azure App Configuration and Key Vaults.

## Getting Started

### Prerequisites
- GitHub account with API token
- FormSubmit account
- Azure account (for production)

### Steps to Run the Project
1. **Clone the Repository**:
    ```bash
    git clone https://github.com/Sane7222/ePortfolio.git
    ```
2. **Configure your GitHub API token**:
    - Update the app settings with your GitHub API Token.
3. **Configure your FormSubmit email address**:
    - Update the `contact.js` file with your FormSubmit endpoint.
4. **Start the Project**:
    ```bash
    dotnet run
    ```

## License
This project is licensed under the MIT License.