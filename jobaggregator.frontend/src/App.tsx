import React, { lazy, Suspense } from "react";
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import { ThemeProvider } from "@mui/material/styles";
import theme from "./theme";
import { Helmet } from "react-helmet";
import { Box, CircularProgress } from "@mui/material";

const Home = lazy(() => import("./pages/Home"));
const SearchResults = lazy(() => import("./pages/SearchResults"));
const JobDetail = lazy(() => import("./pages/JobDetail"));

const App: React.FC = () => {
  return (
    <>
      <Helmet>
        <title>HledačPráce - Najděte si svoji ideální práci</title>
        <meta
          name="description"
          content="Vyhledávejte mezi tisíci pracovních nabídek na jednom místě."
        />
      </Helmet>
      <ThemeProvider theme={theme}>
        <Router>
          <Suspense
            fallback={
              <Box
                sx={{
                  display: "flex",
                  justifyContent: "center",
                  alignItems: "center",
                  height: "100vh",
                }}
              >
                <CircularProgress size={60} />
              </Box>
            }
          >
            <Routes>
              <Route path="/" element={<Home />} />
              <Route path="/search" element={<SearchResults />} />
              <Route path="/job/:id" element={<JobDetail />} />
            </Routes>
          </Suspense>
        </Router>
      </ThemeProvider>
    </>
  );
};

export default App;
