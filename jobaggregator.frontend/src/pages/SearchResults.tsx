import React, { useEffect, useState } from "react";
import { useLocation } from "react-router-dom";
import NavBar from "../components/NavBar";
import SearchBar from "../components/SearchBar";
import { Container, Grid2, Typography, Skeleton } from "@mui/material";
import axios from "axios";
import { JobPosting } from "../models/JobPosting";
import Footer from "../components/Footer";
import JobCard from "../components/JobCard";
import InfiniteScroll from "react-infinite-scroll-component";

const SearchResults: React.FC = () => {
  const location = useLocation();
  const [jobs, setJobs] = useState<JobPosting[]>([]);
  const [pageNumber, setPageNumber] = useState<number>(1);
  const [hasMore, setHasMore] = useState<boolean>(true);
  const [loading, setLoading] = useState<boolean>(true);
  const [queryParams, setQueryParams] = useState<Record<string, string>>({});

  useEffect(() => {
    const params = new URLSearchParams(location.search);
    const queryObj = Object.fromEntries(params.entries());
    setQueryParams(queryObj);
    setJobs([]);
    setPageNumber(1);
    setHasMore(true);
  }, [location.search]);

  const fetchJobs = () => {
    setLoading(true);
    axios
      .get<JobPosting[]>("/JobPostings", {
        params: {
          ...queryParams,
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
        console.error("Error loading job offers:", error);
        setHasMore(false);
        setLoading(false);
      });
  };

  useEffect(() => {
    fetchJobs();
  }, [queryParams]);

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
    <>
      <NavBar />
      <SearchBar />
      <Container sx={{ marginTop: 4 }}>
        <Typography variant="h4" component="div" sx={{ marginBottom: 2 }}>
          Výsledky vyhledávání
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
                <b>Žádné další výsledky</b>
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
      <Footer />
    </>
  );
};

export default SearchResults;
