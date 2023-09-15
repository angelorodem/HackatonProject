import React, { useState, useEffect } from 'react';

 

export default function WikiComponent() {

    const [QnAs, setQnAs] = useState([]);
    const [searchText, setSearchText] = useState([]);
    const [allQnAs, setAllQnAs] = useState([]);
    const [title, setTitle] = useState("DRI Channel");

    useEffect(() => {

        fetch(`https://localhost:64445/ConversationSummary/GetTeamsChannelSummary?channelName=all`, {

            method: 'GET',

            accept: 'text/plain'

        }).then((response) => response.json())

            .then((data) => {

                console.log("data is " + JSON.stringify(data));

                setAllQnAs(data);

            })

            .catch((err) => {

                console.log("Failed with error" + err);

            });

    }, []);

   

    return (

        <div className="wiki-section">

            <div className="wiki-sidebar">

                <h2>Channels</h2>

                <ul>

                    <li>

                        <button
                            onClick={() => {setTitle("DRI Channel Wiki"); setQnAs(allQnAs?.filter((qna) => qna?.channelName === "dri_channel"))}}
                        >DRI Channel</button>

                    </li>

                    <li>

                        <button
                            onClick={() => {setTitle("Engineering Channel Wiki"); setQnAs(allQnAs?.filter((qna) => qna?.channelName === "engineering_channel"))}}

                         >Engineering Channel

                        </button>

                    </li>

                    <li>

                        <button
                          onClick={() => {setTitle("Mathmetics Channel Wiki"); setQnAs(allQnAs?.filter((qna) => qna?.channelName === "mathematics_channel"))}}
                         >Mathemetics  Channel
                        </button>
                    </li>
                    <li>
                            <input type="text" id="searchText" />
                            <input type="button" value="Search Wiki" onClick = {() => { let searchText = document.getElementById('searchText').value; console.log("SearchText" + searchText);setQnAs([]); setSearchText(allQnAs.filter((qna) => qna?.question?.indexOf(searchText) > -1))}}/>
                    </li>
                </ul>

            </div>

            <div className="wiki-main-content" id = "WikiResults">
                <div className="wiki-content">

                   {QnAs && QnAs.length !== 0 && 
                      <h1> {title} </h1>
                   }

                      {QnAs && QnAs.length !== 0 &&  QnAs.map &&  QnAs.map((QnA) => {
                        return (
                            <div key={QnA.conversationId} className="wiki-content-item">
                                <h2 className="wiki-item-title">{QnA.question}</h2>
                                <p className="post-body">{QnA.summary}</p>
                           </div>
                        );

                      })}
                </div>

                <div className="search-wiki-content">
                    {searchText && searchText.length !== 0 && 
                        <h1> Search Results </h1>
                    }
                    {searchText && searchText.length !== 0 &&  searchText?.map &&  searchText?.map((QnA) => {

                        return (

                            <div key={QnA.conversationId} className="wiki-content-item">

                                <h2 className="wiki-item-title">{QnA.question}</h2>

                                <p className="post-body">{QnA.summary}</p>

                            </div>

                        );

                    })}
                </div>

            </div>

        </div>
    )
}
