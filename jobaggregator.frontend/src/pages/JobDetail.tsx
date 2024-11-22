import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import axios from "axios";
import {
  Container,
  Typography,
  Button,
  Grid,
  Card,
  CardContent,
  CardMedia,
  Skeleton,
} from "@mui/material";
import NavBar from "../components/NavBar";
import Footer from "../components/Footer";
import { JobPosting } from "../models/JobPosting";

const JobDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [job, setJob] = useState<JobPosting | null>(null);

  useEffect(() => {
    axios
      .get<JobPosting>(`/JobPostings/${id}`)
      .then((response) => {
        setJob(response.data);
      })
      .catch((error) => {
        console.error("Error loading job offer detail:", error);
      });
  }, [id]);

  if (!job) {
    return (
      <>
        <NavBar />
        <Container sx={{ marginTop: 4 }}>
          <Skeleton variant="rectangular" width="100%" height={400} />
        </Container>
        <Footer />
      </>
    );
  }

  const imageUrl = job.companyLogoUrl || job.portal?.portalLogoUrl || "https://via.placeholder.com/150";

  return (
    <>
      <Container sx={{ marginTop: 4 }}>
        <Grid container spacing={4}>
          <Grid item xs={12} md={8}>
            <Typography variant="h4" component="div" sx={{ marginBottom: 2 }}>
              {job.title}
            </Typography>
            <Typography variant="body1" sx={{ marginBottom: 2 }}>
              <strong>Lokalita:</strong> {job.location}
            </Typography>
            <Typography variant="body1" sx={{ marginBottom: 2 }}>
              <strong>Mzda:</strong> {job.salary}
            </Typography>
            <Typography variant="body1" sx={{ marginBottom: 2 }}>
              <strong>Typ práce:</strong> {job.jobType}
            </Typography>
            <Typography variant="body1" sx={{ marginBottom: 4 }}>
              {job.description}
            </Typography>
            <Button
              variant="contained"
              color="primary"
              href={job.applyUrl}
              target="_blank"
              sx={{
                transition: "background-color 0.3s",
                "&:hover": {
                  backgroundColor: "primary.dark",
                },
              }}
            >
              Ucházet se o pozici
            </Button>
          </Grid>

          <Grid item xs={12} md={4}>
            <Card sx={{ display: "flex", flexDirection: "column", alignItems: "center", textAlign: "center" }}>
              <CardMedia
                component="img"
                image={imageUrl}
                alt={job.companyName}
                sx={{ width: "100%", height: 200, objectFit: "contain" }}
              />
              <CardContent>
                <Typography variant="h6" sx={{ marginTop: 2 }}>
                  {job.companyName}
                </Typography>
                <Typography variant="body2" color="textSecondary">
                  O společnosti
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      </Container>
    </>
  );
};

export default JobDetail;
