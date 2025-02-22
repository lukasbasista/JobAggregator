# Job Aggregator

An advanced job aggregator that collects job postings from multiple sources and presents them in a unified interface. It uses a **React + TypeScript** frontend and a **.NET 8** backend. The application also integrates with a **GPT-based parser** to simplify and scale data extraction.

## Live Demo

[https://hledacprace.eu/](https://hledacprace.eu/)

---

## Features

- **Job Postings Search**: Allows users to search and filter by keywords, location, company name, or job type.
- **Autocomplete Suggestions**: Provides quick suggestions for keywords, locations, company names, and job types.
- **Detailed job view**: Comprehensive details for each job.
- **GPT Integration**: Uses GPT to parse and standardize job postings from various sources for consistent data.
- **Quartz Scheduling**: Periodically runs scraping jobs.
- **Infinite Scrolling**

---

## Tech Stack

### Frontend

- **React**
- **TypeScript**
- **Material UI**
- **Axios** - API calls
- **React Router** - routing

### Backend

- **.NET 8** with C#
- **Entity Framework Core** - data access
- **Microsoft Identity** - user management
- **Quartz.NET** - scheduled scraping
- **Serilog**
- **Playwright** - scraping complex pages
- **GPT** - advanced parsing

---

## Scraping

Scraping runs automatically at predefined intervals using **Quartz.NET**.

- The scraping process is managed by `ScrapingJob`, which runs all configured scrapers.
- The scrapers use HTTPClient (or optionally Playwright) and GPT API to gather and parse job postings from various sources.

---

## Development Setup

### 1. Clone the repository

```bash
git clone https://github.com/lukasbasista/JobAggregator.git
```

### Backend Setup

1. Navigate to the `.Api` folder.
2. Configure your database connection and GPT API key in `appsettings.json`.
4. Run migrations if needed:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

5. Start the API:

```bash
dotnet run
```

### Frontend Setup

1. Navigate to the `jobaggregator.frontend` folder.
2. Install dependencies:

```bash
npm install
```

3. Start the dev server:

```bash
npm start
```

The frontend will run at [http://localhost:3000](http://localhost:3000).

---

## Production Deployment

### Backend

1. Build and publish the .NET 8 project:

```bash
dotnet publish -c Release
```

2. Deploy the output to your server.

### Frontend

1. Build a production bundle:

```bash
npm run build
```

2. Deploy the `build` folder to a static hosting service or your server’s web root.

---

## Notes

- **GPT usage is optional**; you can disable or replace it if you prefer a different parsing method.
- **Playwright may require additional OS dependencies**. If you see missing dependency errors, install them with:

```bash
npx playwright install-deps
```
