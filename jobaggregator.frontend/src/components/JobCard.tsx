import React from "react";
import {
  Card,
  CardContent,
  Typography,
  CardActionArea,
  CardMedia,
} from "@mui/material";
import { JobPosting } from "../models/JobPosting";
import { format } from "date-fns";
import { cs } from "date-fns/locale";
import { useNavigate } from "react-router-dom";

interface JobCardProps {
  job: JobPosting;
}

const JobCard: React.FC<JobCardProps> = ({ job }) => {
  const navigate = useNavigate();

  const handleClick = () => {
    navigate(`/job/${job.jobPostingID}`);
  };

  const imageUrl = job.companyLogoUrl || job.portal?.portalLogoUrl || "https://via.placeholder.com/150";


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
      <CardMedia
          component="img"
          image={imageUrl}
          alt={job.companyName || "Company Logo"}
          sx={{
            height: 140,
            objectFit: "contain",
            objectPosition: "center",
            backgroundColor: "#f5f5f5",
          }}
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
          <Typography
            variant="h6"
            sx={{ color: "green", fontWeight: "bold", mt: 1 }}
          >
            {job.salary}
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
            Přidáno:{" "}
            {format(new Date(job.createdDate), "d. MMMM yyyy", { locale: cs })}
          </Typography>
        </CardContent>
      </CardActionArea>
    </Card>
  );
};

export default JobCard;
