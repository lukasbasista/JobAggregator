import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import axios from "axios";
import {
  Container,
  Typography,
  Button,
  Grid,
  Card,
  CardMedia,
  Skeleton,
  Box,
} from "@mui/material";
import NavBar from "../components/NavBar";
import Footer from "../components/Footer";
import { JobPosting } from "../models/JobPosting";
import DOMPurify from "dompurify";
import LocationOnIcon from "@mui/icons-material/LocationOn";
import SalaryDisplay from "../components/SalaryDisplay";
import JobImage from "../components/JobImage";

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

  const allowedTags = [
    "div",
    "p",
    "b",
    "strong",
    "ul",
    "ol",
    "li",
    "table",
    "thead",
    "tbody",
    "tfoot",
    "tr",
    "th",
    "td",
    "span",
    "h1",
    "h2",
    "h3",
    "h4",
    "h5",
    "br",
  ];

  const sanitizedDescription = DOMPurify.sanitize(job.description, { ALLOWED_TAGS: allowedTags });

  return (
    <>
      <Container sx={{ marginTop: 4 }}>
        <Grid container spacing={4}>
        <Grid item xs={12} md={8}>
            <Typography variant="h4" component="div" sx={{ marginBottom: 2 }}>
              {job.title}
            </Typography>
            {job.location && (
              <Typography
                variant="body1"
                sx={{
                  marginBottom: 2,
                  display: "flex",
                  alignItems: "center",
                }}
              >
                <strong>Lokalita:</strong> {job.location}
                <LocationOnIcon sx={{ marginRight: 1, color: "text.secondary" }} />
              </Typography>
            )}            
            {job.jobType && (
              <Typography variant="body1" sx={{ marginBottom: 2 }}>
                <strong>Typ práce:</strong> {job.jobType}
              </Typography>
            )}
            {(job.salaryFrom || job.salaryTo) && (
              <>
                <Typography variant="subtitle1" sx={{ marginBottom: 1, fontWeight: "bold" }}>
                  Mzda:
                </Typography>
                <SalaryDisplay salaryFrom={job.salaryFrom} salaryTo={job.salaryTo} currency={job.currency} />
              </>
            )}
            <Typography
              variant="body1"
              sx={{ marginBottom: 4 }}
              dangerouslySetInnerHTML={{ __html: sanitizedDescription }}
            />
          </Grid>
          {(job.company || job.applyUrl) && (
            <Grid item xs={12} md={4}>
              <Box sx={{ position: { md: "sticky" }, top: { md: "80px" } }}>
                {job.company && (
                  <Card
                    sx={{
                      display: "flex",
                      flexDirection: "column",
                      alignItems: "flex-start",
                      textAlign: "left",
                      p: 2,
                    }}
                  >
                    <JobImage
                      companyLogo={job.company?.logoUrl}
                      portalLogo={job.portal?.portalLogoUrl}
                      alt={job.company?.companyName || "Company Logo"}
                    />
                    {job.company.companyName && (
                      <Typography variant="h6" align="center" gutterBottom sx={{ width: "100%" }}>
                        {job.company.companyName}
                      </Typography>
                    )}
                    <Grid container spacing={1} justifyContent="flex-start">
                      {job.company.foundedYear && (
                        <Grid item xs={12}>
                          <Typography variant="body2">
                            <strong>Rok založenia:</strong> {job.company.foundedYear}
                          </Typography>
                        </Grid>
                      )}
                      {job.company.headquarters && (
                        <Grid item xs={12}>
                          <Typography variant="body2">
                            <strong>Sídlo:</strong> {job.company.headquarters}
                          </Typography>
                        </Grid>
                      )}
                      {job.company.industry && (
                        <Grid item xs={12}>
                          <Typography variant="body2">
                            <strong>Odvetvie:</strong> {job.company.industry}
                          </Typography>
                        </Grid>
                      )}
                      {job.company.numberOfEmployees && (
                        <Grid item xs={12}>
                          <Typography variant="body2">
                            <strong>Počet zamestnancov:</strong> {job.company.numberOfEmployees}
                          </Typography>
                        </Grid>
                      )}
                      {job.company.description && (
                        <Typography variant="body2" align="center" sx={{ mb: 2, width: "100%" }}>
                          {job.company.description}
                        </Typography>
                      )}
                      {job.company.websiteUrl && (
                        <Box sx={{ width: "100%", display: "flex", justifyContent: "center" }}>
                          <Button
                            variant="outlined"
                            color="primary"
                            href={job.company.websiteUrl}
                            target="_blank"
                            sx={{ mb: 2, fontSize: "0.875rem" }}
                          >
                            Navštíviť web
                          </Button>
                        </Box>
                      )}
                    </Grid>
                  </Card>
                )}
                {job.applyUrl && (
                  <Box
                    sx={{
                      mt: { xs: 0, md: 2 },
                      display: "flex", // zmena: použite flex pre všetky veľkosti
                      justifyContent: "center",
                      position: { xs: "fixed", md: "static" },
                      bottom: { xs: 0, md: "auto" },
                      left: { xs: 0, md: "auto" },
                      right: { xs: 0, md: "auto" },
                      p: { xs: 0, md: 0 },
                      backgroundColor: "transparent",
                      zIndex: { xs: 1000, md: "auto" },
                    }}
                  >
                    <Button
                      variant="contained"
                      color="primary"
                      size="large"
                      href={job.applyUrl}
                      target="_blank"
                      sx={{
                        px: { xs: 2, md: 4 },
                        py: { xs: 1, md: 2 },
                        fontSize: { xs: "0.875rem", md: "1.125rem" },
                        borderRadius: "20px",
                        transition: "all 0.3s",
                        "&:hover": {
                          backgroundColor: "primary.dark",
                        },
                      }}
                    >
                      Ucházet se o pozici
                    </Button>
                  </Box>
                )}
              </Box>
            </Grid>
          )}
        </Grid>
      </Container>
    </>
  );
};

export default JobDetail;
