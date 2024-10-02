'use client'
import AuctionCard from "@/app/auctions/AuctionCard";
import AppPagination from "@/app/components/AppPagination";
import { getData } from '@/app/actions/auctionActions';
import { Auction, PagedResult } from '@/app/types';
import { useEffect, useState } from "react";
import Filters from "./Filters";
import { useParamsStore } from "@/hooks/userParamsStore";
import { useShallow } from "zustand/shallow";
import qs from 'query-string';
import EmptyFilter from "../components/EmptyFilter";

export default function Listings() {
    //const data = await getData();
    // const [auctions, setAuctions] = useState<Auction[]>([]);
    // const [pageCount, setPageCount] = useState<number>(0);
    // const [pageNumber, setPageNumber] = useState<number>(1);
    // const [pageSize, setPageSize] = useState<number>(4);


    const [data, setData] = useState<PagedResult<Auction>>();
    const params = useParamsStore(useShallow(state => ({
        pageNumber: state.pageNumber,
        pageSize: state.pageSize,
        searchTerm: state.searchTerm,
        orderBy: state.orderBy,
        filterBy: state.filterBy
    })));

    const setParams = useParamsStore(state => state.setParams);
    const url = qs.stringifyUrl({ url: '', query: params });

    function setPageNumber(pageNumber: number) {
        setParams({ pageNumber });
    }

    useEffect(() => {
        getData(url).then(data => {
            setData(data);
        })
    }, [url])

    if (!data) {
        return (<h3>Loading...</h3>)
    }

    return (
        <>
            <Filters />
            {data.totalCount === 0 ? (
                <EmptyFilter showReset />
            ) : (
                <>
                    <div className="grid grid-cols-4 gap-6">
                        {data && data.result.map((auction: Auction) => (
                            <AuctionCard auction={auction} key={auction.id} />
                        ))}
                    </div>
                    <div className="flex justify-center mt-4">
                        <AppPagination currentPage={params.pageNumber} pageCount={data.pageCount} pageChanged={setPageNumber} />
                    </div>
                </>
            )}
        </>
    )
}