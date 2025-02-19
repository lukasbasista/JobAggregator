import React, { useState } from "react";
import {
  Card,
  CardContent,
  Typography,
  CardActionArea,
  CardMedia,
} from "@mui/material";
import { JobPosting } from "../models/JobPosting";
import { differenceInCalendarDays, format } from "date-fns";
import { cs } from "date-fns/locale";
import { useNavigate } from "react-router-dom";
import SalaryDisplay from "./SalaryDisplay";
import JobImage from "./JobImage";

interface JobCardProps {
  job: JobPosting;
}

const JobCard: React.FC<JobCardProps> = ({ job }) => {
  const navigate = useNavigate();

  const createdDate = new Date(job.createdDate);
  const diffDays = differenceInCalendarDays(new Date(), createdDate);
  const formattedDate =
    diffDays === 0 ? "Dnes" : diffDays === 1 ? "Včera" : format(createdDate, "d. MMMM yyyy", { locale: cs });



  const handleClick = () => {
    navigate(`/job/${job.jobPostingID}`);
  };

  return (
    <Card
      sx={{
        height: "100%",
        display: "flex",
        flexDirection: "column",
        transition: "transform 0.2s",
        "&:hover": {
          transform: "translateY(-8px)",
          boxShadow: 6,
        },
      }}
    >
      <CardActionArea onClick={handleClick} sx={{ flexGrow: 1 }}>
      <JobImage
          companyLogo={job.company?.logoUrl}
          portalLogo={job.portal?.portalLogoUrl}
          alt={job.company?.companyName || "Company Logo"}
        />
        <CardContent>
          <Typography variant="h5" component="div" gutterBottom>
            {job.title}
          </Typography>
          <Typography variant="subtitle1" color="text.secondary">
            {job.companyName}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            {job.location}
          </Typography>
          <SalaryDisplay salaryFrom={job.salaryFrom} salaryTo={job.salaryTo} currency={job.currency} />
          <Typography variant="body2" color="text.secondary" sx={{ mt: 1, textAlign: "right" }}>
            Přidáno: {formattedDate}
          </Typography>
        </CardContent>
      </CardActionArea>
    </Card>
  );
};

export default JobCard;
