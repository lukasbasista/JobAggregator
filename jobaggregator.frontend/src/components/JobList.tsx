import React, { useEffect, useState, useCallback, useMemo } from "react";
import axios from "axios";
import { Container, Grid, Typography } from "@mui/material";
import { JobPosting } from "../models/JobPosting";
import JobCard from "./JobCard";
import InfiniteScroll from "react-infinite-scroll-component";
import SkeletonLoader from "./SkeletonLoader";

interface JobListProps {
  fetchUrl: string;
  title: string;
  queryParams?: Record<string, any>;
}

const JobList: React.FC<JobListProps> = ({ fetchUrl, title, queryParams }) => {
  const [jobs, setJobs] = useState<JobPosting[]>([]);
  const [pageNumber, setPageNumber] = useState<number>(1);
  const [hasMore, setHasMore] = useState<boolean>(true);
  const [initialLoading, setInitialLoading] = useState<boolean>(true);

  const pageSize = 9;

  const memoizedQueryParams = useMemo(() => queryParams || {}, [queryParams]);

  const fetchJobs = useCallback(
    async (page: number) => {
      try {
        const response = await axios.get<JobPosting[]>(fetchUrl, {
          params: {
            ...memoizedQueryParams,
            pageNumber: page,
            pageSize: pageSize,
          },
        });
        if (response.data.length === 0) {
          setHasMore(false);
        } else {
          setJobs((prevJobs) => (page === 1 ? response.data : [...prevJobs, ...response.data]));
          setPageNumber(page + 1);
        }
      } catch (error) {
        console.error("Error loading job offers:", error);
        setHasMore(false);
      } finally {
        setInitialLoading(false);
      }
    },
    [fetchUrl, memoizedQueryParams]
  );

  useEffect(() => {
    setJobs([]);
    setPageNumber(1);
    setHasMore(true);
    setInitialLoading(true);
    fetchJobs(1);
  }, [fetchUrl, memoizedQueryParams, fetchJobs]);

  const loadMoreJobs = () => {
    fetchJobs(pageNumber);
  };

  return (
    <Container sx={{ marginTop: 4 }}>
      <Typography variant="h4" component="div" sx={{ marginBottom: 2 }}>
        {title}
      </Typography>
      {initialLoading ? (
        <SkeletonLoader pageSize={pageSize} />
      ) : (
        <InfiniteScroll
          dataLength={jobs.length}
          next={loadMoreJobs}
          hasMore={hasMore}
          loader={<SkeletonLoader pageSize={pageSize} />}
          endMessage={
            <Typography variant="body1" align="center" sx={{ marginTop: 2 }}>
              <b>Žádné další inzeráty</b>
            </Typography>
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

export default JobList;
