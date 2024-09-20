import React, { useEffect, useState } from "react";
import axios from "axios";
import { Container, Typography, Skeleton, Grid2 } from "@mui/material";
import { JobPosting } from "../models/JobPosting";
import JobCard from "./JobCard";
import InfiniteScroll from "react-infinite-scroll-component";

const LatestJobList: React.FC = () => {
  const [jobs, setJobs] = useState<JobPosting[]>([]);
  const [pageNumber, setPageNumber] = useState<number>(1);
  const [hasMore, setHasMore] = useState<boolean>(true);
  const [loading, setLoading] = useState<boolean>(true);

  const fetchJobs = () => {
    setLoading(true);
    axios
      .get<JobPosting[]>("/JobPostings/Latest", {
        params: {
          pageNumber: pageNumber,
          pageSize: 9,
        },
      })
      .then((response) => {
        if (response.data.length === 0) {
          setHasMore(false);
        } else {
          setJobs((prevJobs) => [...prevJobs, ...response.data]);
          setPageNumber((prevPage) => prevPage + 1);
        }
        setLoading(false);
      })
      .catch((error) => {
        console.error("Error loading the latest job offers:", error);
        setHasMore(false);
        setLoading(false);
      });
  };

  useEffect(() => {
    fetchJobs();
  }, []);

  const renderSkeletons = () => {
    return (
      <Grid2 container spacing={4}>
        {[...Array(9)].map((_, index) => (
          <Grid2 key={index} size={{ xs: 12, sm: 6, md: 4 }}>
            <Skeleton variant="rectangular" width="100%" height={150} />
            <Skeleton variant="text" />
            <Skeleton variant="text" />
          </Grid2>
        ))}
      </Grid2>
    );
  };

  return (
    <Container sx={{ marginTop: 4 }}>
      <Typography variant="h4" component="div" sx={{ marginBottom: 2 }}>
        Nové inzeráty
      </Typography>
      {loading && pageNumber === 1 ? (
        renderSkeletons()
      ) : (
        <InfiniteScroll
          dataLength={jobs.length}
          next={fetchJobs}
          hasMore={hasMore}
          loader={
            <Grid2 container spacing={4}>
              {renderSkeletons()}
            </Grid2>
          }
          endMessage={
            <p style={{ textAlign: "center" }}>
              <b>Žádné ďalší inzeráty</b>
            </p>
          }
        >
          <Grid2 container spacing={4}>
            {jobs.map((job) => (
              <Grid2 key={job.jobPostingID} size={{ xs: 12, sm: 6, md: 4 }}>
                <JobCard job={job} />
              </Grid2>
            ))}
          </Grid2>
        </InfiniteScroll>
      )}
    </Container>
  );
};

export default LatestJobList;
