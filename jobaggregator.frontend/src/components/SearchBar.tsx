import React, { useEffect, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import {
  Container,
  Grid2,
  TextField,
  Button,
  Box,
  Autocomplete,
  InputAdornment,
  debounce,
} from "@mui/material";
import axios from "axios";
import { Business, LocationOn, Search, Work } from "@mui/icons-material";

const SearchBar: React.FC = () => {
  const [keywords, setKeywords] = useState<string>("");
  const [location, setLocation] = useState<string>("");
  const [companyName, setCompanyName] = useState<string>("");
  const [jobType, setJobType] = useState<string>("");

  const [keywordOptions, setKeywordOptions] = useState<string[]>([]);
  const [locationOptions, setLocationOptions] = useState<string[]>([]);
  const [companyNameOptions, setCompanyNameOptions] = useState<string[]>([]);
  const [jobTypeOptions, setJobTypeOptions] = useState<string[]>([]);
  const navigate = useNavigate();
  const currentLocation = useLocation();

  useEffect(() => {
    const params = new URLSearchParams(currentLocation.search);
    console.log(params);
    const keywordsParam = params.get("keywords") || "";
    const locationParam = params.get("location") || "";
    const companyNameParam = params.get("companyName") || "";
    const jobTypeParam = params.get("jobType") || "";

    setKeywords(keywordsParam);
    setLocation(locationParam);
    setCompanyName(companyNameParam);
    setJobType(jobTypeParam);
  }, [currentLocation.search]);

  const fetchKeywordOptions = debounce((value: string) => {
    axios
      .get<string[]>("/JobPostings/Autocomplete/Keywords", {
        params: { term: value },
      })
      .then((response) => {
        setKeywordOptions(response.data);
      })
      .catch((error) => {
        console.error("Error loading keyword suggestions:", error);
      });
  }, 300);

  const fetchLocationOptions = debounce((value: string) => {
    axios
      .get<string[]>("/JobPostings/Autocomplete/Locations", {
        params: { term: value },
      })
      .then((response) => {
        setLocationOptions(response.data);
      })
      .catch((error) => {
        console.error("Error loading keyword suggestions:", error);
      });
  }, 300);

  const fetchCompanyNameOptions = debounce((value: string) => {
    axios
      .get<string[]>("/JobPostings/Autocomplete/CompanyNames", {
        params: { term: value },
      })
      .then((response) => {
        setCompanyNameOptions(response.data);
      })
      .catch((error) => {
        console.error("Error loading company suggestions:", error);
      });
  }, 300);

  const fetchJobTypeOptions = debounce((value: string) => {
    axios
      .get<string[]>("/JobPostings/Autocomplete/JobTypes", {
        params: { term: value },
      })
      .then((response) => {
        setJobTypeOptions(response.data);
      })
      .catch((error) => {
        console.error("Error loading job type suggestions:", error);
      });
  }, 300);

  const handleSearch = () => {
    const params = new URLSearchParams();
    if (keywords) params.append("keywords", keywords);
    if (location) params.append("location", location);
    if (companyName) params.append("companyName", companyName);
    if (jobType) params.append("jobType", jobType);

    navigate(`/search?${params.toString()}`);
  };

  const handleKeyPress = (event: React.KeyboardEvent) => {
    if (event.key === "Enter") {
      handleSearch();
    }
  };

  return (
    <Container sx={{ marginTop: 4 }}>
      <Box sx={{ padding: 2, backgroundColor: "#f5f5f5", borderRadius: 2 }}>
        <Grid2 container spacing={2} alignItems="center">
          <Grid2 size={{ xs: 12, sm: 6, md: 3 }}>
            <Autocomplete
              freeSolo
              options={keywordOptions}
              inputValue={keywords}
              onInputChange={(event, value) => {
                setKeywords(value);
                fetchKeywordOptions(value);
              }}
              renderInput={(params) => (
                <TextField
                  {...params}
                  label="Klíčová slova"
                  variant="outlined"
                  fullWidth
                  onKeyPress={handleKeyPress}
                  InputProps={{
                    ...params.InputProps,
                    startAdornment: (
                      <InputAdornment position="start">
                        <Search />
                      </InputAdornment>
                    ),
                  }}
                />
              )}
            />
          </Grid2>
          <Grid2 size={{ xs: 12, sm: 6, md: 3 }}>
            <Autocomplete
              freeSolo
              options={locationOptions}
              inputValue={location}
              onInputChange={(event, value) => {
                setLocation(value);
                fetchLocationOptions(value);
              }}
              renderInput={(params) => (
                <TextField
                  {...params}
                  label="Lokalita"
                  variant="outlined"
                  fullWidth
                  onKeyPress={handleKeyPress}
                  InputProps={{
                    ...params.InputProps,
                    startAdornment: (
                      <InputAdornment position="start">
                        <LocationOn />
                      </InputAdornment>
                    ),
                  }}
                />
              )}
            />
          </Grid2>
          <Grid2 size={{ xs: 12, sm: 6, md: 3 }}>
            <Autocomplete
              freeSolo
              options={companyNameOptions}
              inputValue={companyName}
              onInputChange={(event, value) => {
                setCompanyName(value);
                fetchCompanyNameOptions(value);
              }}
              renderInput={(params) => (
                <TextField
                  {...params}
                  label="Název společnosti"
                  variant="outlined"
                  fullWidth
                  onKeyPress={handleKeyPress}
                  InputProps={{
                    ...params.InputProps,
                    startAdornment: (
                      <InputAdornment position="start">
                        <Business />
                      </InputAdornment>
                    ),
                  }}
                />
              )}
            />
          </Grid2>
          <Grid2 size={{ xs: 12, sm: 6, md: 3 }}>
            <Autocomplete
              freeSolo
              options={jobTypeOptions}
              inputValue={jobType}
              onInputChange={(event, value) => {
                setJobType(value);
                fetchJobTypeOptions(value);
              }}
              renderInput={(params) => (
                <TextField
                  {...params}
                  label="Typ práce"
                  variant="outlined"
                  fullWidth
                  onKeyPress={handleKeyPress}
                  InputProps={{
                    ...params.InputProps,
                    startAdornment: (
                      <InputAdornment position="start">
                        <Work />
                      </InputAdornment>
                    ),
                  }}
                />
              )}
            />
          </Grid2>
          <Grid2 size={{ xs: 12 }} sx={{ textAlign: "right" }}>
            <Button variant="contained" color="primary" onClick={handleSearch}>
              Vyhledat
            </Button>
          </Grid2>
        </Grid2>
      </Box>
    </Container>
  );
};

export default SearchBar;
