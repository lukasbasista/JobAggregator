import React, { useEffect, useState, useCallback } from "react";
import axios from "axios";
import {
  Container,
  Typography,
  Skeleton,
  Grid,
  Card,
  CardContent,
  CardActionArea,
} from "@mui/material";
import { JobPosting } from "../models/JobPosting";
import JobCard from "./JobCard";
import InfiniteScroll from "react-infinite-scroll-component";

const LatestJobList: React.FC = () => {
  const [jobs, setJobs] = useState<JobPosting[]>([]);
  const [pageNumber, setPageNumber] = useState<number>(1);
  const [hasMore, setHasMore] = useState<boolean>(true);
  const [initialLoading, setInitialLoading] = useState<boolean>(true);
  const [prefetchedJobs, setPrefetchedJobs] = useState<JobPosting[] | null>(null);

  const pageSize = 9;

  const fetchJobs = useCallback(async () => {
    if (prefetchedJobs) {
      setJobs((prevJobs) => [...prevJobs, ...prefetchedJobs]);
      setPrefetchedJobs(null);
      setPageNumber((prevPage) => prevPage + 1);
      prefetchJobs(pageNumber + 1);
    } else {
      try {
        const response = await axios.get<JobPosting[]>("/JobPostings/Latest", {
          params: {
            pageNumber: pageNumber,
            pageSize: pageSize,
          },
        });
        if (response.data.length === 0) {
          setHasMore(false);
        } else {
          setJobs((prevJobs) => [...prevJobs, ...response.data]);
          setPageNumber((prevPage) => prevPage + 1);
          prefetchJobs(pageNumber + 1);
        }
      } catch (error) {
        console.error("Error loading the latest job offers:", error);
        setHasMore(false);
      }
    }
    setInitialLoading(false);
  }, [pageNumber, prefetchedJobs]);

  const prefetchJobs = async (nextPageNumber: number) => {
    try {
      const response = await axios.get<JobPosting[]>("/JobPostings/Latest", {
        params: {
          pageNumber: nextPageNumber,
          pageSize: pageSize,
        },
      });
      if (response.data.length === 0) {
        setHasMore(false);
      } else {
        setPrefetchedJobs(response.data);
      }
    } catch (error) {
      console.error("Error prefetching job offers:", error);
      setHasMore(false);
    }
  };

  useEffect(() => {
    const initialize = async () => {
      await fetchJobs();
    };
    initialize();
  }, []);

  const renderSkeletons = () => {
    return (
      <Grid container spacing={4}>
        {[...Array(pageSize)].map((_, index) => (
          <Grid item key={index} xs={12} sm={6} md={4}>
            <Card
              sx={{
                height: "100%",
                display: "flex",
                flexDirection: "column",
              }}
            >
              <CardActionArea sx={{ flexGrow: 1 }}>
                <Skeleton variant="rectangular" height={140} />
                <CardContent>
                  <Skeleton variant="text" height={30} width="80%" />
                  <Skeleton variant="text" width="60%" />
                  <Skeleton variant="text" width="40%" />
                  <Skeleton variant="text" width="50%" />
                </CardContent>
              </CardActionArea>
            </Card>
          </Grid>
        ))}
      </Grid>
    );
  };

  return (
    <Container sx={{ marginTop: 4 }}>
      <Typography variant="h4" component="div" sx={{ marginBottom: 2 }}>
        Nové inzeráty
      </Typography>
      {initialLoading ? (
        renderSkeletons()
      ) : (
        <InfiniteScroll
          dataLength={jobs.length}
          next={fetchJobs}
          hasMore={hasMore}
          loader={renderSkeletons()}
          endMessage={
            <p style={{ textAlign: "center" }}>
              <b>Žádné další inzeráty</b>
            </p>
          }
        >
          <Grid container spacing={4}>
            {jobs.map((job) => (
              <Grid item key={job.jobPostingID} xs={12} sm={6} md={4}>
                <JobCard job={job} />
              </Grid>
            ))}
          </Grid>
        </InfiniteScroll>
      )}
    </Container>
  );
};

export default LatestJobList;
