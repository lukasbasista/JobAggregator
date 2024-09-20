import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import axios from "axios";
import {
  Container,
  Typography,
  Button,
  Grid2,
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
        console.error("Chyba pri načítaní detailu pracovnej ponuky:", error);
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

  return (
    <>
      <NavBar />
      <Container sx={{ marginTop: 4 }}>
        <Grid2 container spacing={4}>
          <Grid2 size={{ xs: 12, md: 8 }}>
            <Typography variant="h4" component="div" sx={{ marginBottom: 2 }}>
              {job.title}
            </Typography>
            <Typography
              variant="h6"
              color="text.secondary"
              sx={{ marginBottom: 2 }}
            >
              {job.companyName}
            </Typography>
            <Typography variant="body1" sx={{ marginBottom: 2 }}>
              <strong>Lokalita:</strong> {job.location}
            </Typography>
            <Typography variant="body1" sx={{ marginBottom: 2 }}>
              <strong>Plat:</strong> {job.salary}
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
              Uchádzať sa o pozíciu
            </Button>
          </Grid2>
          <Grid2 size={{ xs: 12, md: 4 }}>
            <Card>
              <CardMedia
                component="img"
                image="https://via.placeholder.com/150"
                alt={job.companyName}
              />
              <CardContent>
                <Typography variant="h6">O spoločnosti</Typography>
              </CardContent>
            </Card>
          </Grid2>
        </Grid2>
      </Container>
      <Footer />
    </>
  );
};

export default JobDetail;
