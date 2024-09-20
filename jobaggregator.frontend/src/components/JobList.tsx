import React, { useEffect, useState } from "react";
import axios from "axios";
import {
  Container,
  Grid2,
  Card,
  CardContent,
  Typography,
  Button,
  TextField,
} from "@mui/material";
import { JobPosting } from "../models/JobPosting";

const JobList: React.FC = () => {
  const [jobs, setJobs] = useState<JobPosting[]>([]);
  const [keywords, setKeywords] = useState<string>("");
  const [location, setLocation] = useState<string>("");
  const [companyName, setCompanyName] = useState<string>("");
  const [jobType, setJobType] = useState<string>("");

  const searchJobs = () => {
    axios
      .get<JobPosting[]>("/JobPostings", {
        params: {
          keywords: keywords || undefined,
          location: location || undefined,
          companyName: companyName || undefined,
          jobType: jobType || undefined,
        },
      })
      .then((response) => {
        setJobs(response.data);
      })
      .catch((error) => {
        console.error("Error loading job offers:", error);
      });
  };

  useEffect(() => {
    searchJobs();
  });

  const handleSearch = () => {
    searchJobs();
  };

  return (
    <Container sx={{ marginTop: 4 }}>
      <Typography variant="h4" component="div" sx={{ marginBottom: 2 }}>
        Vyhledávání inzerátů
      </Typography>
      <Grid2 container spacing={2}>
        <Grid2 size={{ xs: 12, sm: 6, md: 3 }}>
          <TextField
            label="Název pozice"
            variant="outlined"
            fullWidth
            value={keywords}
            onChange={(e) => setKeywords(e.target.value)}
          />
        </Grid2>
        <Grid2 size={{ xs: 12, sm: 6, md: 3 }}>
          <TextField
            label="Lokalita"
            variant="outlined"
            fullWidth
            value={location}
            onChange={(e) => setLocation(e.target.value)}
          />
        </Grid2>
        <Grid2 size={{ xs: 12, sm: 6, md: 3 }}>
          <TextField
            label="Název společnosti"
            variant="outlined"
            fullWidth
            value={companyName}
            onChange={(e) => setCompanyName(e.target.value)}
          />
        </Grid2>
        <Grid2 size={{ xs: 12, sm: 6, md: 3 }}>
          <TextField
            label="Typ práce"
            variant="outlined"
            fullWidth
            value={jobType}
            onChange={(e) => setJobType(e.target.value)}
          />
        </Grid2>
        <Grid2 size={{ xs: 12 }} sx={{ textAlign: "right" }}>
          <Button variant="contained" color="primary" onClick={handleSearch}>
            Vyhledat
          </Button>
        </Grid2>
      </Grid2>

      <Grid2 container spacing={4} sx={{ marginTop: 2 }}>
        {jobs.map((job) => (
          <Grid2 key={job.jobPostingID} size={{ xs: 12, sm: 6, md: 4 }}>
            <Card>
              <CardContent>
                <Typography variant="h5" component="div">
                  {job.title}
                </Typography>
                <Typography sx={{ mb: 1.5 }} color="text.secondary">
                  {job.companyName}
                </Typography>
                <Typography variant="body2">{job.location}</Typography>
                <Typography variant="body2">{job.salary}</Typography>
              </CardContent>
              <Button size="small" href={`/job/${job.jobPostingID}`}>
                Více informací
              </Button>
            </Card>
          </Grid2>
        ))}
      </Grid2>
    </Container>
  );
};

export default JobList;
